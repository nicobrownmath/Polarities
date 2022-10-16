sampler uImage0 : register(s0); //input image
sampler uImage1 : register(s1); //extra stuff
sampler uImage2 : register(s2);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float2 uTargetPosition;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float2 uImageSize2;
float4 uShaderSpecificData;

float4 EclipxieSun(float2 coords : TEXCOORD0) : COLOR0
{
    float2 normalizedCoords = coords * 2 - 1;
    float theta = atan2(normalizedCoords.y, normalizedCoords.x);
    float rMultiplier = tex2D(uImage1, float2(uShaderSpecificData.x, theta / 6.28313853071)).x;
    float r = max(length(normalizedCoords) - uShaderSpecificData.z, 0) / (1 - uShaderSpecificData.y * rMultiplier) / (1 - uShaderSpecificData.z);
    if (r <= 1) {
        return tex2D(uImage0, float2(r, 0));
    }
    else {
        return float4(0, 0, 0, 0);
    }
}

float4 TriangleFade(float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    if (abs(coords.y * 2 - 1) <= coords.x) {
        return float4(baseColor.xyz, baseColor.w * (1 - coords.x));
    }
    else {
        return float4(0, 0, 0, 0);
    }
}

float4 WarpZoomRipple(float2 coords : TEXCOORD0) : COLOR0
{
    float2 normalizedCoords = coords * 2 - 1;
    float r = (length(normalizedCoords) - uShaderSpecificData.x) / (1 - uShaderSpecificData.x);
    if (r <= 1 && r >= 0) {
        float theta = atan2(normalizedCoords.y, normalizedCoords.x);
        return float4(0.5 - 0.5 * cos(theta), 0.5 - 0.5 * sin(theta), 0.5, 1) * (4 * r * (1 - r) * uShaderSpecificData.y);
    }
    else {
        return float4(0, 0, 0, 0);
    }
}

float4 RadialOverlay(float2 coords : TEXCOORD0) : COLOR0
{
    float2 normalizedCoords = coords * 2 - 1;
    float theta = atan2(normalizedCoords.y, normalizedCoords.x);
    float r = length(normalizedCoords);
    if (r <= 1) {
        float4 baseColor = tex2D(uImage0, coords);
        float4 overlayColor = tex2D(uImage1, float2(log(r) / uShaderSpecificData.x + uShaderSpecificData.y, theta / 6.28313853071)) * (1 - uShaderSpecificData.z) + float4(uShaderSpecificData.zzzz);
        return baseColor * overlayColor * uOpacity;
    }
    else {
        return float4(0, 0, 0, 0);
    }
}

float4 DrawAsSphere(float2 coords : TEXCOORD0) : COLOR0
{
    float2 normalizedCoords = coords * 2 - 1;
    float r = length(normalizedCoords);
    if (r <= 1) {
        float theta = atan2(normalizedCoords.y, normalizedCoords.x);
        float brightnessMultiplier = dot(float3(normalizedCoords, sqrt(1 - r * r)), uShaderSpecificData.xyz);
        return float4(tex2D(uImage0, normalizedCoords * asin(r) / (r * 2)).xyz * uColor * brightnessMultiplier, 1);
    }
    else {
        return float4(0, 0, 0, 0);
    }
}

float4 DrawWavy(float2 coords : TEXCOORD0) : COLOR0
{
    float4 output = float4(0, 0, 0, 0);

    float absW = uShaderSpecificData.w > 0 ? uShaderSpecificData.w : -uShaderSpecificData.w;
    float heightMult = coords.x * (1 - coords.x) * 2 / (1 + absW);
    float fluctuation = (sin(coords.x * uShaderSpecificData.y + uShaderSpecificData.z) + uShaderSpecificData.w) * heightMult;
    float y = coords.y * uShaderSpecificData.x - (uShaderSpecificData.x - 1) * (fluctuation + 0.5);
    if (y >= 0 && y <= 1) {
        output += tex2D(uImage0, float2(coords.x, y));
    }
    float fluctuation2 = (sin(coords.x * uShaderSpecificData.y + uShaderSpecificData.z + 3.14159) + uShaderSpecificData.w) * heightMult;
    float y2 = coords.y * uShaderSpecificData.x - (uShaderSpecificData.x - 1) * (fluctuation2 + 0.5);
    if (y2 >= 0 && y2 <= 1) {
        output += tex2D(uImage0, float2(coords.x, y2));
    }

    return output;
}

technique Technique1
{
    pass TriangleFadePass
    {
        PixelShader = compile ps_2_0 TriangleFade();
    }
    pass EclipxieSunPass
    {
        PixelShader = compile ps_2_0 EclipxieSun();
    }
    pass WarpZoomRipplePass
    {
        PixelShader = compile ps_2_0 WarpZoomRipple();
    }
    pass RadialOverlayPass
    {
        PixelShader = compile ps_2_0 RadialOverlay();
    }
    pass DrawAsSpherePass
    {
        PixelShader = compile ps_2_0 DrawAsSphere();
    }
    pass DrawWavyPass
    {
        PixelShader = compile ps_2_0 DrawWavy();
    }
}