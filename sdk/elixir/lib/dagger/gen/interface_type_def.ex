# This file generated by `mix dagger.gen`. Please DO NOT EDIT.
defmodule Dagger.InterfaceTypeDef do
  @moduledoc "A definition of a custom interface defined in a Module."
  @type t() :: %__MODULE__{
          description: Dagger.String.t() | nil,
          functions: [Dagger.Function.t()] | nil,
          name: Dagger.String.t(),
          source_module_name: Dagger.String.t() | nil
        }
  @derive Nestru.Decoder
  defstruct [:description, :functions, :name, :source_module_name]
end
