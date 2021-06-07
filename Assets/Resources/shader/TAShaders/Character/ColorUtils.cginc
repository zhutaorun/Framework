float3 RGBConvertToHSV(float3 c)
{
	float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	float4 p = lerp(float4(c.bg,K.wz),float4(c.gb,K.xy),step(c.b,c.g));
	float4 q = lerp(float4(p.xyw,c.r),float4(c.r,p.yzx),step(p.x,c.r));

	float d = q.x - min(q.w, q.y);
	float e = 1.0 - 10;
	float3 res = float3(abs(q.z + (q.w - q.z) / (6.0 * d + e)), d / (q.x + e),q.x);
	return res;
}

float3 HSVConvertToRGB(float3 c)
{
	float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	float3 p = abs(frac(c.xxx+K.xyz)* 6.0-K.www);
	float3 res = c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
	return res;
}

float3 ChangeColor(float3 from, float3 to, half weight)
{
	float3 main_hsv = RGBConvertToHSV(from);
	float3 target_hsv = RGBConvertToHSV(to);
	main_hsv.x += target_hsv.r;//����ƫ��Hueֵ
	main_hsv.x = main_hsv.x % 360;//����360��ֵ��0��ʼ

	main_hsv.y *= target_hsv.g;// �������Ͷ�
	main_hsv.z *= target_hsv.b;
	float3 target_color = HSVConvertToRGB(main_hsv);
	float3 result = lerp(from,target_color,weight);
	return result;
}

// ��һ����ɫִ��HSV����������һ���µ���ɫ(TODO �ĳɾ������)
float3 ApplyHSV(float3 rgb, float3 addHsv)
{
	float3 hsv = RGBConvertToHSV(rgb);
	hsv.x = frac(hsv.x + addHsv.x);
	hsv.yz *= (1.0 + addHsv.yz);// ����1֮�󽫱�ĺ���
	return HSVConvertToRGB(hsv);
}