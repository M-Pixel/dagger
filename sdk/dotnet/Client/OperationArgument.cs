using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

namespace Dagger;

/// <summary>User-provided argument to a query as a forward-linked list.</summary>
record OperationArgument
(
	string Name,
	OperationArgumentValue Value,
	OperationArgument? Next = null
)
{
	public void PrimeLinkedListInParallel()
	{
		OperationArgument? argument = this;
		do
		{
			argument.Value.Prime();
			argument = argument.Next;
		} while (argument != null);
	}

	/// <summary>Format argument sequence into GraphQL query format.</summary>
	/// <example><c>foo:1,bar:"baz",</c></example>
	public async Task SerializeLinkedList(StringBuilder queryOut)
	{
		OperationArgument? argument = this;
		while (true)
		{
			await argument.SerializeSingle(queryOut);
			argument = argument.Next;
			if (argument == null)
				return;
			queryOut.Append(',');
		}
	}

	/// <summary>Format argument into GraphQL query format.</summary>
	/// <example><c>foo:1</c></example>
	public ValueTask SerializeSingle(StringBuilder queryOut)
	{
		queryOut.Append(Name);
		queryOut.Append(':');
		return Value.Serialize(queryOut);
	}
}

/// <summary>Implements the serialization of a single operation argument value.</summary>
/// <remarks>
///		While possible to determine "HowToSerialize" by analyzing the type of the raw Value through reflection, the code
///		for doing so is substantially more complicated, and less efficient.  The efficiency hardly matters, but that's
///		exactly why eliminating a few bytes isn't worth the resulting addition in complexity and ambiguity elsewhere.
///		This approach also enables the encapsulation of mutative argument resolution (e.g. resolve sub-query, lazy
///		enumerate).
/// </remarks>
abstract class OperationArgumentValue
{
	public virtual void Prime() {}
	public abstract ValueTask Serialize(StringBuilder queryOut);
}

/// <summary>Value is a string with complex characters - surround with quotes and escape.</summary>
class StringOperationArgumentValue : OperationArgumentValue
{
	private readonly string _value;

	internal StringOperationArgumentValue(string value)
	{
		_value = value;
	}

	public override ValueTask Serialize(StringBuilder queryOut)
	{
		// Use JSON serializer to accomplish proper escaping for special characters
		queryOut.Append(JsonSerializer.Serialize(_value));
		return ValueTask.CompletedTask;
	}
}

/// <summary>Value is a nested array of <see cref="OperationArgument"/>s - surround with {} and recurse.</summary>
class ObjectOperationArgumentValue : OperationArgumentValue
{
	private readonly OperationArgument _firstField;


	internal ObjectOperationArgumentValue(OperationArgument inputObject)
	{
		_firstField = inputObject;
	}


	public override void Prime()
	{
		_firstField.PrimeLinkedListInParallel();
	}

	public override async ValueTask Serialize(StringBuilder queryOut)
	{
		queryOut.Append('{');
		await _firstField.SerializeLinkedList(queryOut);
		queryOut.Append('}');
	}
}

class TrueOperationArgumentValue : OperationArgumentValue
{
	internal static readonly TrueOperationArgumentValue instance = new();

	public override ValueTask Serialize(StringBuilder queryOut)
	{
		queryOut.Append("true");
		return ValueTask.CompletedTask;
	}
}

class FalseOperationArgumentValue : OperationArgumentValue
{
	internal static readonly TrueOperationArgumentValue instance = new();

	public override ValueTask Serialize(StringBuilder queryOut)
	{
		queryOut.Append("false");
		return ValueTask.CompletedTask;
	}
}

/// <summary>Value has a simple representation that should not be demarcated.</summary>
class EnumOperationArgumentValue<T> : OperationArgumentValue
{
	private readonly T _value;


	public EnumOperationArgumentValue(T value)
	{
		_value = value;
	}


	public override ValueTask Serialize(StringBuilder queryOut)
	{
		queryOut.Append(_value);
		return ValueTask.CompletedTask;
	}
}

static class EnumOperationArgumentValue
{
	/// <summary>
	///		Allows codegen to be simpler by leveraging automatic type deduction, which works no function calls but not
	///		constructors.
	/// </summary>
	public static EnumOperationArgumentValue<T> Create<T>(T value) => new(value);

	public static OperationArgumentValue Create(bool value) =>
		value ? TrueOperationArgumentValue.instance : FalseOperationArgumentValue.instance;
}

/// <summary>Value is a <see cref="ObjectClient"/> object that needs to be substituted with an ID string.</summary>
class ReferenceOperationArgumentValue : OperationArgumentValue
{
	private ObjectClient? _value;
	private Task<string>? _valueTask;


	internal ReferenceOperationArgumentValue(ObjectClient value)
	{
		_value = value;
	}


	public override void Prime() => GetPrimeTask();

	public override async ValueTask Serialize(StringBuilder queryOut)
	{
		queryOut.Append('"');
		queryOut.Append(await GetPrimeTask());
		queryOut.Append('"');
	}

	private Task<string> GetPrimeTask()
	{
		lock (this)
		{
			if (_valueTask == null)
			{
				_valueTask = _value!.Compute();
				_value = null;
			}
			return _valueTask;
		}
	}
}

class ArrayOperationArgumentValue<T> : OperationArgumentValue
{
	private IEnumerable<T>? _values;
	private Task<IList<OperationArgumentValue>>? _typifiedValues;
	private readonly Func<T, OperationArgumentValue> _valueTypifier;


	public ArrayOperationArgumentValue
	(
		IEnumerable<T> values,
		Func<T, OperationArgumentValue> valueTypifier
	)
	{
		_values = values;
		_valueTypifier = valueTypifier;
	}


	public override void Prime() => GetTypifiedValue();

	public override async ValueTask Serialize(StringBuilder queryOut)
	{
		queryOut.Append('[');
		foreach (OperationArgumentValue element in await GetTypifiedValue())
		{
			await element.Serialize(queryOut);
			queryOut.Append(',');
		}
		queryOut[^1] = ']'; // Replace final trailing comma
	}

	private Task<IList<OperationArgumentValue>> GetTypifiedValue()
	{
		lock (this)
		{
			if (_typifiedValues == null)
			{
				_typifiedValues = TypifyValues(_values!);
				_values = null;
			}
			return _typifiedValues;
		}
	}

	private async Task<IList<OperationArgumentValue>> TypifyValues(IEnumerable<T> values)
	{
		await Task.Yield();
		// If we don't know how many elements will be added, an ImmutableList.Builder has the most scalable Add
		IList<OperationArgumentValue> result = values switch
		{
			IReadOnlyList<T> knownQuantity => new List<OperationArgumentValue>(knownQuantity.Count),
			_ => ImmutableList.CreateBuilder<OperationArgumentValue>()
		};
		foreach (T element in values)
			result.Add(_valueTypifier(element));
		return result;
	}
}

static class ArrayOperationArgumentValue
{
	public static ArrayOperationArgumentValue<T> Create<T>(IEnumerable<T> value,
		Func<T, OperationArgumentValue> valueTypifier) => new(value, valueTypifier);
}

record Operation
(
	string Name,
	OperationArgument? Arguments = null
);

static class QueryTreeExtensions
{
	public static ImmutableList<Operation> Add
	(
		this ImmutableList<Operation> queryTree,
		string operationName,
		OperationArgument? firstArgument = null
	)
		=> queryTree.Add(new Operation(operationName, firstArgument));
}
