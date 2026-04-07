---
title: Shader Graph là không đủ ? thử HLSL nhé ?
description: HLSL là cái gì ? có ăn được không ?
author: aros
date: 2025-2-15 00:00:00 +0700
categories: [Tutorial, Shader, Vietnamese]
tags: [tutorial, shader, vietnamese]
pin: true
math: true
mermaid: true
---

Lời đầu mình muốn nói trong bài blog này là cảm ơn đã bấm vào bài blog giật tít đầu này, mong là nó sẽ giúp ích gì đó cho mọi người.

Trong bài viết này chúng ta sẽ thử vọc về cấu tạo HLSL và cùng nhau tạo ra 1 hiệu ứng đơn giản nhưng cực kỳ ấn tượng để làm quen với HLSL.

## Shader Graph và giới hạn của nó 

Nếu mà các bạn từng mò qua về shader graph thì sẽ thấy nó rất là tiện trong việc sử dụng và thân thiện với người dùng

Tất nhiên là nó vẫn rất là đáng tin tuy nhiên nếu muốn làm 1 cái hiệu ứng gì đó nó hoành tá tràng hơn nữa như là ........ một lá bài không gian chẳng hạn ?

![showcase-full-optimize](https://github.com/user-attachments/assets/923d2806-7bfe-4209-a4be-8c0850173746)

Đối với shader graph chay thì chắc là chịu chết, còn nếu dùng node custom Function thì ta sẽ phải đào sâu hơn thế và cụ thể chính là HLSL!!!

Đây chính là cách mà bộ công cụ shader graph thường hay dùng để lấp liếm cho những thứ mà không hề có node sẵn,
tất nhiên là còn 1 cách nữa đó là sử dụng những plugin tương tự shader graph như Amplify Shader, 
nhưng khi chúng ta tới với những hiệu ứng phức tạp hơn như volumetric fog thì những giới hạn tương tự cũng sẽ xảy ra.

Vậy, làm cách nào để chúng ta có thể tạo được những thứ kinh khủng hơn thế ?

---

## Lý Thuyết
Xin giới thiệu với các bạn, đây là HLSL (High-Level Shading Language)

Cấu tạo của 1 shader khi nhìn vào cũng khá là đơn giản

Giờ hãy thử tạo 1 shader mặc định của unity để dễ hình dung hơn

### ***Tạo Shader cơ bản***
1. **Tạo Shader Mới**:  

   - Trong Unity, chuyển đến tab **Project** (thường ở phía dưới màn hình).  
   - Nhấp chuột phải vào thư mục **Assets** hoặc thư mục con mà bạn muốn lưu shader.  
   - Chọn **Create > Shader > Unlit Shader**.
  
![select shader](https://github.com/user-attachments/assets/77ae7625-d081-4975-88b0-d33b301ace40)

2. **Đặt Tên Cho Shader**:  

   - Sau khi chọn xong, Unity sẽ tạo ra một file shader mới.  
   - Đặt tên file shader là `BasicShader.shader` hoặc `BasicShader`.

3. **Mở Shader Vừa Tạo**:  

   - Nhấp đúp chuột vào file `BasicShader.shader` để mở nó bằng Code IDE (Visual Studio hoặc VSCode).

4. **Cấu Trúc Shader Ban Đầu**:  

    Lúc này, code của shader sẽ trông như sau (đây là mẫu cơ bản Unity cung cấp):

    ```csharp
    Shader "Unlit/BasicShader"
    {
        Properties
        {
            _MainTex ("Texture", 2D) = "white" {}
        }
        SubShader
        {
            Tags { "RenderType"="Opaque" }
            LOD 100

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                // make fog work
                #pragma multi_compile_fog

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    UNITY_FOG_COORDS(1)
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;

                v2f vert (appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    UNITY_TRANSFER_FOG(o,o.vertex);
                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    // sample the texture
                    fixed4 col = tex2D(_MainTex, i.uv);
                    // apply fog
                    UNITY_APPLY_FOG(i.fogCoord, col);
                    return col;
                }
                ENDCG
            }
        }
    }
   ```

   Giờ hãy cùng phân tích qua cấu trúc của 1 file Shader nhé, với 1 vài quẹt xóa đi chúng ta sẽ có:

   ```csharp
    Shader "Custom/BasicShader"
    {
        Properties
        {
            _MainTex ("Texture", 2D) = "white" {}
        }

        SubShader
        {
            Cull Off ZWrite Off ZTest Always    //thuộc tính mẫu ngoài có thể dùng trong shader
            LOD 200                             //thuộc tính mẫu ngoài có thể dùng trong shader

            Tags { "RenderType"="Opaque" }
            
            Pass
            {
                CGPROGRAM

                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                struct appdata
                { };

                struct v2f
                { };

                sampler2D _MainTex;
                float4 _MainTex_ST;

                v2f vert (appdata v)
                {
                }

                fixed4 frag (v2f i) : SV_Target
                {
                }

                ENDCG
            }
        }

        
        Fallback "Standard"
    }
```

### Cấu trúc của Shader

Đây chính là cấu trúc cơ bản của 1 file shader:

- Properties: nơi show ra các giá trị view trên inspector, tùy ý, không nhất thiết phải có
- SubShader: Khu vực chứa các Pass (cái này sẽ còn được nhắc lại trong 1 bài về Replacement Shader)
- Pass: Chứa các bước thực thi cho vertex shader hoặc fragment shader (Surface Shader không phải 1 shader lẻ, sẽ được làm rõ trong 1 blog về low-level graphic programming)
- Thuộc tính render (không có trong file gen mặc định): Các thuộc tính này thiết lập trước CGPROGRAM (ZWrite, ZTest, LOD, Cull, etc....)
- CGPROGRAM/ENDCG (HLSLPROGRAM/ENDHLSL): chứa code cho các phép tính toán đồ họa 

**📝 NOTE:**  
> Lý do có 2 loại là CG với HLSL thì đây cùng là ngôn ngữ cho shader của nhà Nvidia, HLSL là đồ mới ngon hơn và CG là đồ cũ

> Hiện tại chúng ta có HLSL sẽ mạnh hơn cho DirectX (đồ của nhà Microsoft), GLSL cho OpenGL (cái api graphic này giờ hơi đuối so với Vulkan), CG thì hỗ trợ cả 2.

> HLSL có thể hỗ trợ mọi loại Render pipeline của Unity, còn CG thì nhịn với URP và HDRP, tuy nhiên thì migrate rất là dễ nên không đáng lo.

**💡 TIP:**
HLSLINCLLUDE và CGINCLUDE có thể được sử dụng để làm gọn code thay vì tống tất cả vào CGPROGRAM/HLSLPROGRAM

```csharp
Shader "Hidden/MyCoolCustomPostProcessingShader"
{
    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float4 _MainTex_TexelSize;
        
        struct v2f
        {
        };

        v2f Vert (AttributesDefault v)
        {
        }

        float4 Frag(v2f input) : SV_Target
        {
        }
    
    ENDHLSL
    
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

                #pragma vertex Vert
                #pragma fragment Frag

            ENDHLSL
        }
    }
}
```

- Fallback: qua bước xử lý trong Pass thì tự động mò tới shader default để render


Ok, ổn định tinh thần sau khi bị nhồi nhét rồi chứ ? giờ là tới lúc ứng dụng luôn nè

## Thực Hành
Mớ lý thuyết thế là đủ rồi, thực hành nhỉ ? 1 lá bài không gian
Chúng ta sẽ sử dụng Built-in Render pipeline của Unity

![showcase-full-optimize](https://github.com/user-attachments/assets/12792d06-cf69-4123-8579-72f8032a3fbf)

### Cơ chế

Đầu tiên hãy nói về mặt ý tưởng

Khi render ra một tấm hình hay góc nhìn của camera, mỗi pixel trên màn hình sẽ chứa nhiều giá trị, trong đó có 1 giá trị gọi là stencil

Đây là 1 shader chỉ cho phép render nếu như những pixel trên màn hình có số stencil hợp với điều kiện thì cho Pass cái shader (tức là cho xem mấy món có stencil tương ứng)

Nghe hơi lằng nhằng nhưng nói chung là giống Mask Effect, pixel trong vùng mask thì lộ, ngoài vùng thì giấu

Đối với shader của tấm kính mask chúng ta sẽ dùng Basic Shader vừa tạo ở trên và thêm vào trong code 

```csharp
Stencil
{
    ref 1
    comp Always
    pass replace
}
```

Nó mang nghĩa là với tất cả các pixel có giá trị stencil = 1 thì nó cho pass là replace (thay thế các pixel bằng thứ được duyệt qua shader này)
vậy là ta sẽ có:

```csharp
Shader "Unlit/BasicShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        //Phần mới thêm vào :D
        Stencil
        {
            ref 1
            comp Always
            pass replace
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
```

### Setup Cơ bản
Chuột phải vào file shader và **Create > Material** và gắn vào tấm kính mask

Đối với những món muốn được nhìn thông qua tấm mask chúng ta sẽ dùng shader standard làm ví dụ.
Chúng ta có thể chèn dòng này vào bất kì shader nào mình muốn (miễn là shader đó đang chưa đụng tới Stencil Buffer)

**Tạo Shader Standard Mới**:  
   - Trong Unity, chuyển đến tab **Project** (thường ở phía dưới màn hình).  
   - Nhấp chuột phải vào thư mục **Assets** hoặc thư mục con mà bạn muốn lưu shader.
   - Chọn **Create > Shader > Standard Surface Shader**.
   - Đặt tên tùy ý, ở trong này mình sẽ đặt là **MaskObject**

Thêm vào chỉ xuất hiện khi giá trị Stencil trùng khi so sánh với tấm mask:

```csharp
    Stencil
    {
        ref 1
        comp equal
    }
```

Tổng kết lại ta sẽ có:

```csharp
Shader "Custom/MaskObject"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Stencil
        {
            ref 1
            comp equal
        }

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
```

Chuột phải vào file shader và **Create > Material** và gắn rồi gắn cái Material mới tạo lên những object muốn hiển thị

### Sửa Lỗi và nâng cấp

#### Multi Stencil Channel
Vì mình muốn làm nhiều hơn là 1 mặt cho lá bài này, cho nên để cho phép sử dụng lại giá trị này với nhiều material hơn nên chúng ta sẽ tinh chỉnh một chút

Trước hết là thay mọi dòng "ref 1" trong **cả 2 shader** bằng (Nhớ là cả 2 shader đấy, cái gì quan trọng nhắc lại)

```csharp
     Ref[_StencilMask]
```

Thêm giá trị ở Properties để cho phép Material sửa qua inspector

```csharp
    Properties
    {
        //etc....
        _StencilMask("Stencil Mask", Int) = 1
    }
```

Giờ với setup tương tự, 2 mặt 2 materials cho mask và số stencil khác nhau và setup những đồ vật của mỗi hướng có stencil tương ứng và đây là thành quả

![basic](https://github.com/user-attachments/assets/843e8f7f-4f38-4e76-9e26-f6b9066b94bc)

Hmmmmmm, cũng khá là hay, bây giờ chúng ta hãy loại bỏ màu của tấm Mask bằng việc loại bỏ màu sắc của nó với câu lệnh set thuộc tính

```csharp
    ColorMask 0
```

#### Flickering Material (Render Queue)

Với giá trị là 0 thì mọi màu trong cả 4 kênh màu RGBA đều sẽ bị lược bỏ, và ngoài ra còn 1 lưu ý nữa, bạn có thể thấy là nó đang nháy nháy

![showcase](https://www.flickr.com/photos/152961556@N08/54369187402/in/dateposted-public/)

Nguyên do là thứ tự render của chúng trong Render Queue đang bằng nhau nên máy tính không biết vẽ cái nào trước

Chỉ cần đẩy số Render Queue của object hoặc cái cổng mask bé hơn là được

![mask](https://github.com/user-attachments/assets/bec8e1a9-7c54-4795-9707-a261d9b6a367)
![obj](https://github.com/user-attachments/assets/f665e3cc-2b62-49b6-94fa-82ffdb4bf01b)

#### Surface Clipping (ZWrite)  

Chỉ còn 1 cái lỗi nhỏ xíu nữa thôi, có thể thấy là nếu chúng ta để món này ở đằng sau dù đã được lọc màu thì nó vẫn bị che mất

Lý do là vì trong camera nó vẫn coi như pixel của lá bài này phải nằm ở phía trên (do nó đứng gần hơn với camera) 

Vậy thì chúng ta chỉ cần đánh dấu là skip không vẽ tấm Mask này nữa là xong 

```csharp
    Zwrite off
```

![1](https://github.com/user-attachments/assets/810ea5fc-beb0-484a-ae55-88ca283bb4dd)
![2](https://github.com/user-attachments/assets/b7dfef85-198e-48ad-971d-fe8930cfbb37)

#### Extra (Sky Dome)

Ở đây để tạo ra 1 cái bầu trời trong lá bài, ta có thể sử dụng 1 kĩ thuật khá cũ gọi là sky dome (object vòm trời)

Nó là 1 object với phía mặt chỉ thấy được từ bên trong, hình khối cầu

![skydome](https://github.com/user-attachments/assets/b425e727-f44d-4bfc-a6b6-0c77820767bd)

Chỉ cần 1 shader unlit đi kèm với câu lệnh về Stencil như với MaskObject là hiệu ứng sẽ hoàn thành

![3D_view_op](https://github.com/user-attachments/assets/38ce3200-9da4-4b29-bd75-682a5d567402)

---

### Các script được sử dụng trong bài blog:

[MaskObject.shader](https://github.com/ArosMailWork/arosmailwork.github.io/blob/main/_posts/Script/HLSLBlog/MaskObject.shader)

[Mask.shader](https://github.com/ArosMailWork/arosmailwork.github.io/blob/main/_posts/Script/HLSLBlog/Mask.shader)

[AutoRotate.cs](https://github.com/ArosMailWork/arosmailwork.github.io/blob/main/_posts/Script/HLSLBlog/AutoRotate.cs)
