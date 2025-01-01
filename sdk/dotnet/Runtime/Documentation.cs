using System.Text;

namespace Dagger.Runtime;

using System.Collections.Immutable;
using System.Xml;

readonly record struct ElementDocumentation
{
	public string? Summary { get; init; }
	public IImmutableDictionary<string, ElementDocumentation>? Members { get; init; }


	private class ParseState
	{
		public ImmutableDictionary<string, ElementDocumentation>.Builder AssemblyMembersBuilder =
			ImmutableDictionary.CreateBuilder<string, ElementDocumentation>();
		public string TypeName = "";
		public string TypeSummary = "";
		public ImmutableDictionary<string, ElementDocumentation>.Builder TypeMembersBuilder =
			ImmutableDictionary.CreateBuilder<string, ElementDocumentation>();
	}

	public static async Task<ElementDocumentation> Parse
	(
		Stream stream,
		CancellationToken cancellationToken = default
	)
	{
		using var reader = XmlReader.Create
		(
			stream,
			new XmlReaderSettings
			{
				Async = true,
				IgnoreWhitespace = true
			}
		);

		ParseState state = new();
		while (await reader.ReadAsync().ConfigureAwait(false))
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (reader.NodeType != XmlNodeType.Element || reader.Name != "member")
				continue;

			string? elementName = reader.GetAttribute("name");
			if (string.IsNullOrEmpty(elementName) || elementName.Contains('`') || elementName[1] != ':')
				continue;

			switch (elementName[0])
			{
				case 'T':
				{
					CommitType(ref state);
					state.TypeName = elementName[2..];
					state.TypeSummary = await ParseMember(reader, null, "member", cancellationToken);
					cancellationToken.ThrowIfCancellationRequested();
					break;
				}

				// We might encounter a member here if there is a documented member of an undocumented type
				case 'M':
				case 'F':
				case 'P':
				{
					int ultimateSeparatorIndex = elementName.LastIndexOf('.');
					string typeName = elementName[2..ultimateSeparatorIndex];
					if (state.TypeName != typeName)
						CommitType(ref state);
					state.TypeName = typeName;

					string memberName = elementName[(elementName.LastIndexOf('.') + 1)..];
					if (elementName[0] == 'M')
					{
						var parameterDocsBuilder = ImmutableDictionary.CreateBuilder<string, ElementDocumentation>();
						string summary = await ParseMember(reader, parameterDocsBuilder, "member", cancellationToken);
						cancellationToken.ThrowIfCancellationRequested();
						state.TypeMembersBuilder.Add
						(
							memberName,
							new ElementDocumentation{ Summary = summary, Members = parameterDocsBuilder.ToImmutable() }
						);
					}
					else
					{
						string summary = await ParseMember(reader, null, "member", cancellationToken);
						cancellationToken.ThrowIfCancellationRequested();
						state.TypeMembersBuilder.Add(memberName, new ElementDocumentation{ Summary = summary });
					}
					break;
				}
			}
			break;
		}
		CommitType(ref state);

		return new ElementDocumentation{ Members = state.AssemblyMembersBuilder.ToImmutable() };
	}

	private static void CommitType(ref ParseState state)
	{
		if (state.TypeName == "")
			return;

		state.AssemblyMembersBuilder.Add
		(
			state.TypeName,
			new ElementDocumentation{ Summary = state.TypeSummary, Members = state.TypeMembersBuilder.ToImmutable() }
		);
		state.TypeName = "";
		state.TypeSummary = "";
		state.TypeMembersBuilder.Clear();
	}

    private static async Task<string> ParseMember
    (
        XmlReader reader,
        ImmutableDictionary<string, ElementDocumentation>.Builder? parameterBuilder,
        string outerElementName,
        CancellationToken cancellationToken
    )
    {
        StringBuilder summary = new();
        bool containsPrecedingParagraph = false;

        while (await reader.ReadAsync())
        {
	        cancellationToken.ThrowIfCancellationRequested();
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
	                if (reader.Name == "p")
	                {
		                if (containsPrecedingParagraph)
			                summary.Append("\n\n");
	                }
	                else if (reader.Name == "br")
                        summary.Append('\n');
	                else if (reader is { Name: "param", IsEmptyElement: false })
	                {
		                string? parameterName = reader.GetAttribute("name");
		                if (string.IsNullOrEmpty(parameterName))
			                continue;
		                parameterBuilder?.Add
			            (
				            parameterName,
				            new ElementDocumentation
					        {
						        Summary = await ParseMember(reader, null, "param", cancellationToken)
						    }
				        );
	                }
	                break;

                case XmlNodeType.Text:
	                containsPrecedingParagraph = true;
                    summary.Append((await reader.GetValueAsync()).Trim());
                    break;

                case XmlNodeType.EndElement:
	                if (reader.Name == "p")
		                containsPrecedingParagraph = true;
	                else if (reader.Name == outerElementName)
		                return summary.ToString();
	                break;
            }
        }

        return string.Concat(summary);
    }
}
