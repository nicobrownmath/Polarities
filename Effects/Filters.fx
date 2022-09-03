sampler uImage0 : register(s0); //world image
sampler uImage1 : register(s1); //extra stuff
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize0;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

float4 ScreenWarp(float2 coords : TEXCOORD0) : COLOR0
{
    return tex2D(uImage0, coords + ((tex2D(uImage1, coords / uZoom).xy) * 255.0 - float2(128.0, 128.0)) / uScreenResolution);
}

technique Technique1
{
    pass ScreenWarpPass
    {
        PixelShader = compile ps_2_0 ScreenWarp();
    }
}