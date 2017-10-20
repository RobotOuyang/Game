package com.mirrortech.gamehall.wxapi;

import java.io.ByteArrayOutputStream;
import java.io.EOFException;
import org.json.JSONException;
import org.json.JSONObject;

import com.mirrortech.gamehall.R;
import com.tencent.mm.sdk.constants.ConstantsAPI;
import com.tencent.mm.sdk.modelbase.BaseReq;
import com.tencent.mm.sdk.modelbase.BaseResp;
import com.tencent.mm.sdk.modelmsg.SendAuth;
import com.tencent.mm.sdk.modelmsg.SendAuth.Resp;
import com.tencent.mm.sdk.modelmsg.SendMessageToWX;
import com.tencent.mm.sdk.modelmsg.WXImageObject;
import com.tencent.mm.sdk.modelmsg.WXMediaMessage;
import com.tencent.mm.sdk.modelmsg.WXWebpageObject;
import com.tencent.mm.sdk.modelpay.PayReq;
import com.tencent.mm.sdk.openapi.IWXAPI;
import com.tencent.mm.sdk.openapi.IWXAPIEventHandler;
import com.tencent.mm.sdk.openapi.WXAPIFactory;
import com.unity3d.player.UnityPlayer;

import android.app.Activity;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.os.Bundle;
import android.util.Log;

public class WXEntryActivity extends Activity  implements IWXAPIEventHandler {
	private IWXAPI api;
	
	void RegWXChat() {	
		String APP_ID = getString(R.string.WX_APP_ID);
		api = WXAPIFactory.createWXAPI(this, APP_ID, false);
		api.registerApp(APP_ID);
		api.handleIntent(getIntent(), this);
	}
	
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        
        RegWXChat();
        
        String type = this.getIntent().getStringExtra("type");
        
        if (type != null)
        {
            if(type.equals("login"))
            {
            	final SendAuth.Req req = new SendAuth.Req();    
    	    	req.scope = this.getIntent().getStringExtra("scope");
    	    	req.state = this.getIntent().getStringExtra("state");
    	    	api.sendReq(req);
            }
            else if(type.equals("pay"))
            {
            	String content = this.getIntent().getStringExtra("json");
            	try {
					JSONObject json = new JSONObject(content);
					PayReq req = new PayReq();
					req.appId			= json.getString("appid");
					req.partnerId		= json.getString("partnerid");
					req.prepayId		= json.getString("prepayid");
					req.nonceStr		= json.getString("noncestr");
					req.timeStamp		= json.getString("timestamp");
					req.packageValue	= json.getString("package");
					req.sign			= json.getString("sign");				
					api.sendReq(req);
				} catch (JSONException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
            }
            else if(type.equals("share_url"))
            {
            	WXWebpageObject webpage = new WXWebpageObject();
            	webpage.webpageUrl = this.getIntent().getStringExtra("url");
            	
            	WXMediaMessage msg = new WXMediaMessage(webpage);
            	msg.title = this.getIntent().getStringExtra("title");
            	msg.description = this.getIntent().getStringExtra("desc");
            	
            	try {
	            	Bitmap thumb = BitmapFactory.decodeFile(this.getIntent().getStringExtra("thumb"));
	            	ByteArrayOutputStream stream = new ByteArrayOutputStream();
	            	thumb.compress(Bitmap.CompressFormat.JPEG, 70, stream);
	            	msg.thumbData = stream.toByteArray();
	            	thumb.recycle();
	            	
	            	SendMessageToWX.Req req = new SendMessageToWX.Req();
	            	req.transaction = "url" + System.currentTimeMillis();
	            	req.message = msg;
	            	req.scene = this.getIntent().getBooleanExtra("timeline", false) ?
	            			SendMessageToWX.Req.WXSceneTimeline :
	            			SendMessageToWX.Req.WXSceneSession;
	            	
	            	api.sendReq(req);
            	}catch (Exception e) {
					e.printStackTrace();
				}
            }
            else if(type.equals("share_pic"))
            {
            	try {
            		int options = 100;
            		WXMediaMessage msg = new WXMediaMessage();
            		ByteArrayOutputStream stream = new ByteArrayOutputStream();
	            	Bitmap bmp = BitmapFactory.decodeFile(this.getIntent().getStringExtra("bmp"));
	            	
	            	WXImageObject imgObj = new WXImageObject(bmp);
					msg.mediaObject = imgObj;
					
					Bitmap thumbBmp = Bitmap.createScaledBitmap(bmp, this.getIntent().getIntExtra("width", 150), 
							this.getIntent().getIntExtra("height", 150), true);
					thumbBmp.compress(Bitmap.CompressFormat.JPEG, options, stream);
					while(stream.size() > 30000)  // 坑壁微信，有大小限制也不在文档里说明
					{
						stream.reset();
						options = options - 10;
						thumbBmp.compress(Bitmap.CompressFormat.JPEG, options, stream);
					}
	            	msg.thumbData = stream.toByteArray();
					bmp.recycle();
					thumbBmp.recycle();

					SendMessageToWX.Req req = new SendMessageToWX.Req();
					req.transaction = "img" + System.currentTimeMillis();
					req.message = msg;
					req.scene = this.getIntent().getBooleanExtra("timeline", false) ?
	            			SendMessageToWX.Req.WXSceneTimeline :
	            			SendMessageToWX.Req.WXSceneSession;
					api.sendReq(req);
					
            	}catch (Exception e) {
					e.printStackTrace();
				}
            }
        }
        this.finish();
    }
    
	// 微信发送请求到第三方应用时，会回调到该方法
	@Override
	public void onReq(BaseReq req) {
		
	}
		
	// 第三方应用发送到微信的请求处理后的响应结果，会回调到该方法
	@Override
	public void onResp(BaseResp resp) {
		Log.d("resp", String.format("id: %d", resp.errCode));
		switch (resp.getType()) {
        	case ConstantsAPI.COMMAND_SENDAUTH:
				if (resp.errCode == BaseResp.ErrCode.ERR_OK)
				{
					UnityPlayer.UnitySendMessage("GameManager","wx_login_message", ((Resp)resp).code);
				}
				break;
        	case ConstantsAPI.COMMAND_PAY_BY_WX:
        		if (resp.errCode != BaseResp.ErrCode.ERR_OK)
        		{
        			UnityPlayer.UnitySendMessage("GameManager","wx_pay_message", resp.errStr);
        		}
        		break;
        	case ConstantsAPI.COMMAND_SENDMESSAGE_TO_WX:
        		if (resp.errCode == BaseResp.ErrCode.ERR_OK)
        		{
        			UnityPlayer.UnitySendMessage("GameManager","wx_share_message", ((SendMessageToWX.Resp)resp).transaction);
        		}
        		break;
        	default:break;
		}
	}
}