﻿using SharpDX.Mathematics.Interop;

namespace VolumeRenderer;

sealed class RayGenerator : IDisposable
{
    private bool _disposed;
    public ShaderResourceView FrontFaceTextureView { get; }
    public ShaderResourceView BackFaceTextureView { get; }
    public RenderTargetView FrontFaceRenderTargetView { get; }
    public RenderTargetView BackFaceRenderTargetView { get; }
    public DepthStencilState FrontFaceDepthStencilState { get; }
    public DepthStencilState BackFaceDepthStencilState { get; }
    public DepthStencilView DepthStencilView { get; }
    public SamplerState SamplerState { get; }

    public RayGenerator(D3DDevice device, int width, int height)
    {
        var textureDescription = new Texture2DDescription
        {
            Width = width,
            Height = height,
            ArraySize = 1,
            BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
            Usage = ResourceUsage.Default,
            CpuAccessFlags = CpuAccessFlags.None,
            Format = Format.R16G16B16A16_Float,
            MipLevels = 1,
            OptionFlags = ResourceOptionFlags.None,
            SampleDescription = new SampleDescription(1, 0)
        };

        using var frontTexture = new Texture2D(device, textureDescription);
        using var backTexture = new Texture2D(device, textureDescription);

        FrontFaceTextureView = new ShaderResourceView(device, frontTexture);
        BackFaceTextureView = new ShaderResourceView(device, backTexture);

        using var depthBuffer = new Texture2D(device, new Texture2DDescription
        {
            Width = width,
            Height = height,
            ArraySize = 1,
            BindFlags = BindFlags.DepthStencil,
            Usage = ResourceUsage.Default,
            CpuAccessFlags = CpuAccessFlags.None,
            Format = Format.D32_Float,
            MipLevels = 0,
            OptionFlags = ResourceOptionFlags.None,
            SampleDescription = new SampleDescription(1, 0),
        });

        DepthStencilView = new DepthStencilView(device, depthBuffer);

        FrontFaceRenderTargetView = new RenderTargetView(device, frontTexture);
        BackFaceRenderTargetView = new RenderTargetView(device, backTexture);

        SamplerState = new SamplerState(device, new SamplerStateDescription
        {
            AddressU = TextureAddressMode.Border,
            AddressV = TextureAddressMode.Border,
            AddressW = TextureAddressMode.Border,
            Filter = Filter.MinMagMipLinear,
            BorderColor = new RawColor4(0, 0, 0, 0)
        });

        var depthStencilStateDesc = new DepthStencilStateDescription
        {
            IsDepthEnabled = true,
            DepthWriteMask = DepthWriteMask.All,
            DepthComparison = Comparison.Less,
            IsStencilEnabled = true,
            StencilReadMask = 0xFF,
            StencilWriteMask = 0xFF,
            FrontFace = new DepthStencilOperationDescription
            {
                Comparison = Comparison.Always,
                FailOperation = StencilOperation.Keep,
                DepthFailOperation = StencilOperation.Increment,
                PassOperation = StencilOperation.Keep
            },
            BackFace = new DepthStencilOperationDescription
            {
                Comparison = Comparison.Always,
                FailOperation = StencilOperation.Keep,
                DepthFailOperation = StencilOperation.Decrement,
                PassOperation = StencilOperation.Keep
            }
        };
        FrontFaceDepthStencilState =  new DepthStencilState(device, depthStencilStateDesc);
        depthStencilStateDesc.FrontFace.DepthFailOperation = StencilOperation.Decrement;
        depthStencilStateDesc.BackFace.DepthFailOperation = StencilOperation.Increment;
        BackFaceDepthStencilState = new DepthStencilState(device, depthStencilStateDesc);

    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            GC.SuppressFinalize(this);
            FrontFaceRenderTargetView?.Dispose();
            BackFaceRenderTargetView?.Dispose();
            FrontFaceTextureView?.Dispose();
            BackFaceTextureView?.Dispose();
            DepthStencilView?.Dispose();
            FrontFaceDepthStencilState?.Dispose();
            BackFaceDepthStencilState?.Dispose();
            SamplerState?.Dispose();
        }
    }

    ~RayGenerator()
    {
        Dispose();
    }
}