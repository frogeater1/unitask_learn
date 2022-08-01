using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class UnitaskTest : MonoBehaviour
{
    public Button button1;
    public TextMeshProUGUI showtext;

    public Button button2;
    public Slider slider;

    public Button button3;
    public Image image;

    public Button button4;

    public PlayerLoopTiming timing = PlayerLoopTiming.Initialization;

    private void Start()
    {
        button1.onClick.AddListener(OnClickBtn1);
        button2.onClick.AddListener(OnClickBtn2);
        button3.onClick.AddListener(OnClickBtn3);
        button4.onClick.AddListener(UniTask.UnityAction(OnClickBtn4));
    }

    private async UniTaskVoid OnClickBtn4()
    {
        var cts = new CancellationTokenSource();
        //cts.CancelAfter(5000);
        cts.CancelAfterSlim(TimeSpan.FromSeconds(5));
        print("1");
        string state;
        var (cancelOrFailed, result) = await UnityWebRequest.Get("https://www.google.com").SendWebRequest().WithCancellation(cts.Token).SuppressCancellationThrow();
        if (!cancelOrFailed)
        {
            state = result.downloadHandler.text[..100];
        }
        else
        {
            state = "取消or超时";
        }

        print(state);
    }

    private async void OnClickBtn1()
    {
        var text = await Resources.LoadAsync<TextAsset>("test");

        showtext.text = ((TextAsset)text).text;
    }

    private async void OnClickBtn2()
    {
        await SceneManager.LoadSceneAsync("Scene2").ToUniTask(
            Progress.Create<float>(f => { slider.value = f; }));
    }

    private async void OnClickBtn3()
    {
        var webRequest =
            UnityWebRequestTexture.GetTexture("https://s1.hdslb.com/bfs/static/jinkela/video/asserts/33-coin-ani.png");
        var result = await webRequest.SendWebRequest();
        var texture = ((DownloadHandlerTexture)result.downloadHandler).texture;
        int spriteCount = 24;
        int perSpriteWidth = texture.width / spriteCount;
        Sprite[] sprites = new Sprite[spriteCount];
        for (int i = 0; i < spriteCount; i++)
        {
            sprites[i] = Sprite.Create(texture, new Rect(new Vector2(perSpriteWidth * i, 0), new Vector2(perSpriteWidth, texture.height)), new Vector2(0.5f, 0.5f));
        }

        float perFrameTime = 0.1f;
        while (true)
        {
            for (int i = 0; i < spriteCount; i++)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(perFrameTime));
                image.sprite = sprites[i];
                image.SetNativeSize();
            }
        }
    }

    private async void OtherFunc()
    {
        bool state = false;
        var a = UniTask.WaitUntil(() => state);
        var b = UniTask.WaitUntil(() => state);
        await UniTask.WhenAll(a, b);
        await UniTask.WhenAny(a, b);


        var a_cancelToken = new CancellationTokenSource(); //创建取消token
        var b_cancelToken = new CancellationTokenSource(); //创建取消token
        CancellationTokenSource.CreateLinkedTokenSource(a_cancelToken.Token, b_cancelToken.Token); //创建联合取消token

        a_cancelToken.Cancel(); //进行取消操作
        a_cancelToken.Dispose(); //释放
        a_cancelToken = new CancellationTokenSource(); //使用一次之后需要再次创建


        var cancelled = await UniTask.NextFrame(a_cancelToken.Token).SuppressCancellationThrow();
        //如果await的Unitask有返回值的话 则 var (cancelled,ret1,ret2...) = await xxx;

        var source = new UniTaskCompletionSource();
        await source.Task;//UniTaskCompletionSource产生的Task可以多次await
        
        source.TrySetResult();//手动设置完成
        source.TrySetException(new SystemException());//手动失败
        source.TrySetCanceled(new CancellationToken());//手动取消
    }
}