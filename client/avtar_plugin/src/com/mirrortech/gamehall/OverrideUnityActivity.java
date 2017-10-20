package com.mirrortech.gamehall;
import java.net.NetworkInterface;
import java.util.Collections;
import java.util.List;

import com.alipay.sdk.app.PayTask;

import com.mirrortech.gamehall.wxapi.WXEntryActivity;
import com.tencent.mm.sdk.openapi.IWXAPI;
import com.tencent.mm.sdk.openapi.WXAPIFactory;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

import android.annotation.TargetApi;
import android.content.ClipData;
import android.content.ClipboardManager;
import android.content.Context;
import android.content.Intent;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;


public class OverrideUnityActivity extends UnityPlayerActivity {

	//public class UnityTestActivity extends Activity {
	Context mContext = null;

	@Override
	public void onCreate(Bundle savedInstanceState) {
			super.onCreate(savedInstanceState);
			mContext = this;
	}
	 
	public void TakePhoto(String path, String file_name)
	{
		 Intent intent = new Intent(mContext, WebViewActivity.class);
		 intent.putExtra("type", "TakePhoto");
		 intent.putExtra("path", path);
		 intent.putExtra("file_name", file_name);
		 this.startActivity(intent);
	}
	
	public void TakeCamera(String path, String file_name)
	{
		 Intent intent = new Intent(mContext, WebViewActivity.class);
		 intent.putExtra("type", "TakeCamera");
		 intent.putExtra("path", path);
		 intent.putExtra("file_name", file_name);
		 this.startActivity(intent);
	}
	
	public void OpenAliPay(String signed_order)
	{		
		final String payInfo = signed_order;
		Runnable payRunnable = new Runnable() {

			@Override
			public void run() {
				// 闂佸搫顑呯�氫即鏁撻悾灞芥ayTask 闁诲海鏁搁、濠囨寘閿燂拷
				PayTask alipay = new PayTask(OverrideUnityActivity.this);
				// 闁荤姴顑呴崯浼村极閵堝缁╂い鏍ㄧ懅鐢盯鏌熼幁鎺戝姎鐟滅増鐩弫宥呯暆閸愩劎顔嗛梺鍛婄懄閻楁寮鐣岊浄婵☆垵鍋愬▔銏ゆ煛鐎ｈ埖瀚�
				String result = alipay.pay(payInfo, true);
				
				UnityPlayer.UnitySendMessage("GameManager","alipay_message", result);
			}
		};

		// 闂婎偄娲ら幊姗�濡磋箛鎿冨殨闁稿本渚楅崝鍕偣鐎ｎ亜鏆熼柡渚婃嫹
		Thread payThread = new Thread(payRunnable);
		payThread.start();		 
	}
	
	public void OpenWXLogin()
	{
		 Intent intent = new Intent(mContext, WXEntryActivity.class);
		 intent.putExtra("type", "login");
		 intent.putExtra("scope", "snsapi_userinfo");
		 intent.putExtra("state", "mirror_tech");
		 this.startActivity(intent);
	}
	
	public int IsWXInstalled()
	{
		String APP_ID = getString(R.string.WX_APP_ID);
		IWXAPI api = WXAPIFactory.createWXAPI(this, APP_ID, false);
		if (api.isWXAppInstalled())
		{
			return 1;
		}
		return 0;
	}
	
	public void OpenWXPay(String pay_json)
	{
		 Intent intent = new Intent(mContext, WXEntryActivity.class);
		 intent.putExtra("type", "pay");
		 intent.putExtra("json", pay_json);
		 this.startActivity(intent);
	}
	
	public void WXShareUrl(String url, String title, String desc, String thumb, boolean is_timeline)
	{
		Intent intent = new Intent(mContext, WXEntryActivity.class);
		intent.putExtra("type", "share_url");
		intent.putExtra("url", url);
		intent.putExtra("title", title);
		intent.putExtra("desc", desc);
		intent.putExtra("timeline", is_timeline);
		intent.putExtra("thumb", thumb);
		this.startActivity(intent);
	}
	
	// 
	public void WXSharePicture(String bmp, int width, int height, boolean is_timeline)
	{
		Intent intent = new Intent(mContext, WXEntryActivity.class);
		intent.putExtra("type", "share_pic");
		intent.putExtra("timeline", is_timeline);
		intent.putExtra("width", width);
		intent.putExtra("height", height);
		intent.putExtra("bmp", bmp);
		this.startActivity(intent);
	}
	
	// 将input输入到剪贴板
	@TargetApi(Build.VERSION_CODES.HONEYCOMB) public void TextToClipboard(String input)
	{
		ClipboardManager myClipboard = (ClipboardManager)getSystemService(CLIPBOARD_SERVICE);
		ClipData myClip = ClipData.newPlainText("text", input);
		myClipboard.setPrimaryClip(myClip);
	}
}