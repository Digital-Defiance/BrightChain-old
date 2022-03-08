namespace BrightChain.Engine.Client;

public class BrightChainSerializationOptions
{
    //
    // Summary:
    //     Create an instance of BrightChainSerializationOptions with default values

    //
    // Summary:
    //     Gets or sets if the serializer should ignore null properties
    //
    // Remarks:
    //     The default value is false
    public bool IgnoreNullValues { get; set; }

    //
    // Summary:
    //     Gets or sets if the serializer should use indentation
    //
    // Remarks:
    //     The default value is false
    public bool Indented { get; set; }
}
