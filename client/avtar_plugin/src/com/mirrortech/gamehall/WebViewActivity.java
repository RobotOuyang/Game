package com.mirrortech.gamehall;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
 
import com.unity3d.player.UnityPlayer;
 
import android.app.Activity;
import android.content.Intent;
import android.graphics.Bitmap;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.provider.MediaStore;
import android.widget.ImageView;
 
public class WebViewActivity extends Activity
{
 
    ImageView imageView = null;  
 
    public static final int PHOTOHRAPH = 1;
    public static final int PHOTOZOOM = 2; 
    public static final int PHOTORESOULT = 3;
 
    public static final String IMAGE_UNSPECIFIED = "image/*";  
 
    public static String PERSISTENT_PATH = null;
    public static String FILE_NAME = null;
    public final static String DATA_URL = "/data/data/";
 
	@Override
	protected void onCreate(Bundle savedInstanceState) {
 
		super.onCreate(savedInstanceState);
 
		setContentView(R.layout.main);
		
		imageView = (ImageView) this.findViewById(R.id.imageID);
 
		String type = this.getIntent().getStringExtra("type");
		FILE_NAME = this.getIntent().getStringExtra("file_name");
		PERSISTENT_PATH = this.getIntent().getStringExtra("path");
 
		if(type.equals("TakeCamera"))
		{
			  Intent intent = new Intent(MediaStore.ACTION_IMAGE_CAPTURE);
              intent.putExtra(MediaStore.EXTRA_OUTPUT, Uri.fromFile(new File(Environment.getExternalStorageDirectory(), FILE_NAME)));
              startActivityForResult(intent, PHOTOHRAPH);
		}else
		{
		      Intent intent = new Intent(Intent.ACTION_PICK, null);
              intent.setDataAndType(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, IMAGE_UNSPECIFIED);
              startActivityForResult(intent, PHOTOZOOM);
		}
    }
	
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (resultCode == RESULT_CANCELED )
        {
            this.finish();
            super.onActivityResult(requestCode, resultCode, data);
            return;
        }
        if (requestCode == PHOTOHRAPH) {
            File picture = new File(Environment.getExternalStorageDirectory() + "/" + FILE_NAME);
            startPhotoZoom(Uri.fromFile(picture));
        }  
 
        if (data == null)
            return;  

        if (requestCode == PHOTOZOOM) {
            startPhotoZoom(data.getData());
        }
        if (requestCode == PHOTORESOULT) {
            Bundle extras = data.getExtras();
            if (extras != null) {  
 
                Bitmap photo = extras.getParcelable("data");
		        imageView.setImageBitmap(photo);  
 
            	try {
            		SaveBitmap(photo);
				} catch (IOException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
            }
        }  
 
        super.onActivityResult(requestCode, resultCode, data);
    }  
 
    public void startPhotoZoom(Uri uri) {
        Intent intent = new Intent("com.android.camera.action.CROP");
      
        intent.setDataAndType(uri, IMAGE_UNSPECIFIED);
        intent.putExtra("crop", "true");
        intent.putExtra("aspectX", 1);
        intent.putExtra("aspectY", 1);
        intent.putExtra("outputX", 300);
        intent.putExtra("outputY", 300);
        intent.putExtra("return-data", true);
        startActivityForResult(intent, PHOTORESOULT);
    }  
 
	public void SaveBitmap(Bitmap bitmap) throws IOException {
 
		FileOutputStream fOut = null;

		try {
			  File destDir = new File(PERSISTENT_PATH);
			  if (!destDir.exists())
			  {
				  destDir.mkdirs();
			  }
 
			fOut = new FileOutputStream(PERSISTENT_PATH + FILE_NAME) ;
		} catch (FileNotFoundException e) {
			e.printStackTrace();
		}
		bitmap.compress(Bitmap.CompressFormat.JPEG, 100, fOut);
		try {
			fOut.flush();
		} catch (IOException e) {
			e.printStackTrace();
		}
		try {
			fOut.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
		UnityPlayer.UnitySendMessage("GameManager","avatar_message", PERSISTENT_PATH + FILE_NAME);
        this.finish();
	}
}