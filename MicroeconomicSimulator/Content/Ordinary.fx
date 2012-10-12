float4x4 World;
float4x4 ViewXProjection;

//-----"Ordinary" Technique-----
//Displays things as they are
struct VSOut
{
    float4 Position : POSITION0;
    float4 Color	: COLOR0;
};

VSOut OrdinaryVS(
	float4 Position : POSITION0
	, float4 Color : COLOR0)
{
	VSOut output = (VSOut) 0;
    output.Position = mul (mul (Position, World), ViewXProjection);
    output.Color = Color;
    
    return output;
}

float4 OrdinaryPS(VSOut input) : COLOR0
{
    return input.Color;
}

technique Ordinary
{
    pass Pass1
    {
        VertexShader = compile vs_1_1 OrdinaryVS();
        PixelShader = compile ps_1_1 OrdinaryPS();
    }
}