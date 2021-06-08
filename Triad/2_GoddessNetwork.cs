using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Net;
using System;
using UnityEngine.UI;
using UnityEngine.Android;

public class GoddessNetwork : MonoBehaviour
{
    public InputField inputiPAddress;

    public int maxPlayer = 2;

    const int Port = 7777;

    [HideInInspector]
    public Goddess goddess;

    IPAddress iPAddress;

    [HideInInspector]
    public bool isServer;

    NetworkClient client;


    // Use this for initialization
    void Start()
    {
        //マイクロフォンの使用許可を取る
        Permission.RequestUserPermission(Permission.Microphone);

        //デバイスのIPアドレスを取得
        iPAddress = IPAddress.Parse(GetDeviceIpAddress());

        //IPアドレスを入力フィールドに反映させる
        inputiPAddress.text = iPAddress.ToString();
    }


    // Update is called once per fram
    void Update()
    {
        FindGoddess();
    }

    bool networking = false;

    void FindGoddess()
    {
        //ARCoreを使用するため、サーバーとの接続が確立してから自動的に女神像が出現する。
        //そのため、毎フレーム女神像がいるかどうかを確認する。
        //もっと多くのオブジェクトを生成する場合はARCoreのコードから直接関数を呼ぶべきであると思う。
        //しかし、ネットの接続環境によって誤差が生じるため、こちらの検索方法を採用。
        if (goddess)
        {
            return;
        }

        GameObject g = GameObject.Find("Goddess(Clone)");
        if (g)
        {
            goddess = g.transform.GetChild(0).GetComponent<Goddess>();
        }
    }

    string GetDeviceIpAddress()
    {
        string ipAddress;
#if UNITY_2018_2_OR_NEWER
        string hostName = Dns.GetHostName();
        IPAddress[] addresses = Dns.GetHostAddresses(hostName);

        ipAddress = "Unknown";
        foreach (IPAddress address in addresses)
        {
            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                ipAddress = address.ToString();
                break;
            }
        }
#else
            ipAddress = Network.player.ipAddress;
#endif
        return ipAddress;
    }

    /// <summary>
    /// 女神像を代入する
    /// </summary>
    public void SetGoddess(Goddess goddess)
    {
        this.goddess = goddess;
    }

    /// <summary>
    /// サーバーを開設する
    /// </summary>
    public void CreateServer()
    {
        goddess.HideQRCord();
        print("サーバー起動");
        NetworkServer.Listen(Port);

        //イベントハンダラーを登録
        print("Handlerを登録");
        NetworkServer.RegisterHandler(MyMsg.audio, OnAudioReceived);
        NetworkServer.RegisterHandler(MyMsg.barrier, OnCrackReceived);
        NetworkServer.RegisterHandler(MyMsg.color, OnWeakPointReceived);
        NetworkServer.RegisterHandler(MyMsg.goddessVoice, OnGoddessReceived);
        NetworkServer.RegisterHandler(MyMsg.purification, OnPurificationReceived);
        NetworkServer.RegisterHandler(MyMsg.respawn, OnRespawnReceived);
        NetworkServer.RegisterHandler(MyMsg.gameStart, OnGameStartReceived);
        NetworkServer.RegisterHandler(MyMsg.time, OnTimeReceived);
        NetworkServer.RegisterHandler(MyMsg.despawn, OnDespawnReceived);
        NetworkServer.RegisterHandler(MyMsg.average, OnAverageReceived);


        print(iPAddress.ToString());

        isServer = true;

        print("バリア生成");
        goddess.VoiceInitialize();
        goddess.SetBarriers();
    }

    /// <summary>
    /// クライアントの生成をリクエストする
    /// </summary>
    public void CreateClientRequest()
    {
        StartCoroutine(IE_CreateClient());
    }

    void CreateClient()
    {
        print("クライアント生成");
        client = new NetworkClient();

        print("クライアントをサーバーへ接続");
        client.Connect(Address.ipAddress, Port);

        //イベントハンダラーを登録
        print("Handlerを登録");
        client.RegisterHandler(MyMsg.audio, OnAudioReceived);
        client.RegisterHandler(MyMsg.barrier, OnCrackReceived);
        client.RegisterHandler(MyMsg.color, OnWeakPointReceived);
        client.RegisterHandler(MyMsg.goddessVoice, OnGoddessReceived);
        client.RegisterHandler(MyMsg.purification, OnPurificationReceived);
        client.RegisterHandler(MyMsg.respawn, OnRespawnReceived);
        client.RegisterHandler(MyMsg.gameStart, OnGameStartReceived);
        client.RegisterHandler(MyMsg.time,OnTimeReceived);
        client.RegisterHandler(MyMsg.despawn, OnDespawnReceived);
        client.RegisterHandler(MyMsg.average, OnAverageReceived);

        isServer = false;
    }

    /// <summary>
    /// サーバークライアント間でのアクションを定義
    /// </summary>
    public static class MyMsg
    {
        public const short audio = MsgType.Highest + 4;
        public const short barrier = MsgType.Highest + 5;
        public const short color = MsgType.Highest + 6;
        public const short goddessVoice = MsgType.Highest + 7;
        public const short purification = MsgType.Highest + 8;
        public const short respawn = MsgType.Highest + 9;
        public const short gameStart = MsgType.Highest + 10;
        public const short time = MsgType.Highest + 11;
        public const short despawn = MsgType.Highest + 12;
        public const short average = MsgType.Highest + 13;

    }

    IEnumerator IE_CreateClient()
    {
        yield return new WaitForSeconds(1);
        while (true)
        {
            CreateClient();
            print("女神像サーバーに接続中");
            yield return new WaitForSeconds(1);
            if (client.isConnected)
            {
                print("女神像サーバーに接続完了");
                SendGoddessVoice();
                SendGameStart();

                break;
            }
        }
        yield break;
    }

    #region 音声の同期

    const int PART_SIZE = 4096;

    float[] data = new float[1];

    void OnAudioReceived(NetworkMessage message)
    {
        AudioData d = message.ReadMessage<AudioData>();
        Array.Resize(ref data, d.size);

        //音声データが非常に大きいため、分割して受信する
        for (int i = d.start; i < d.end; i++)
        {
            data[i] = d.part[i - d.start];

            if (i != d.size - 1) continue;

            AudioClip clip = goddess.audioSource.clip;
            AudioClip newClip = AudioClip.Create(clip.name, d.size, clip.channels, clip.frequency, false);
            newClip.SetData(data, 0);
            goddess.AddClip(d.index, newClip);
        }

        if (isServer) NetworkServer.SendToAll(MyMsg.audio, d);
    }

    /// <summary>
    /// 音声データの送信
    /// </summary>
    /// <param name="index">弱点ポイントのインデックス</param>
    /// <param name="clip">オーディオクリップ</param>
    public void SendAudio(int index, AudioClip clip)
    {
        if (isServer) goddess.AddClip(index, clip);

        //音声データ取得
        float[] d = new float[clip.samples * clip.channels];
        clip.GetData(d, 0);

        //音声データが非常に大きいため、分割して送信する
        int sendLoop = d.Length / PART_SIZE + (d.Length % PART_SIZE > 0 ? 1 : 0);
        for (int i = 0; i < sendLoop; i++)
        {
            int start = i * PART_SIZE;
            int end = (i + 1) * PART_SIZE;
            if (end >= d.Length)
            {
                end = d.Length;
            }

            //分割出来たデータを送信する
            float[] part = new float[PART_SIZE];
            Array.Copy(d, start, part, 0, end - start);
            AudioData s = new AudioData()
            {
                index = index,
                part = part,
                start = start,
                end = end,
                size = d.Length,
                name = clip.name,
                channnels = clip.channels,
                frequency = clip.frequency
            };

            //サーバーの場合はクライアントに送りクライアントの場合はサーバーに送る
            if (isServer)
            {
                NetworkServer.SendToAll(MyMsg.audio, s);
            }
            else
            {
                client.Send(MyMsg.audio, s);
            }
        }
    }

    public class AudioData : MessageBase
    {
        /// <summary>
        /// 弱点ポイントのインデックス
        /// </summary>
        public int index;

        /// <summary>
        /// 分割済みの音声データ
        /// </summary>
        public float[] part;

        /// <summary>
        /// 音声データの開始位置
        /// </summary>
        public int start;

        /// <summary>
        /// 音声データの終了位置
        /// </summary>
        public int end;

        /// <summary>
        /// 音声データのサイズ
        /// </summary>
        public int size;

        /// <summary>
        /// クリップの名前
        /// </summary>
        public string name;

        /// <summary>
        /// クリップのチャンネル
        /// </summary>
        public int channnels;

        /// <summary>
        /// クリップの周波数
        /// </summary>
        public int frequency;
    }

    #endregion

    #region バリアの同期

    public void SendCrack()
    {
        if (isServer)
        {
            //サーバーの場合はひび割れを更新し、クライアントにひび割れの情報を送る
            NetworkServer.SendToAll(MyMsg.barrier, new EmptyMessage());
            goddess.AddCrack();
        }
        else
        {
            //クライアントの場合はサーバーにひび割れの情報を送る
            client.Send(MyMsg.barrier, new EmptyMessage());
        }
    }

    void OnCrackReceived(NetworkMessage message)
    {
        goddess.AddCrack();
        if (isServer)
        {
            //サーバーの場合はクライアントにひび割れの情報を送る
            NetworkServer.SendToAll(MyMsg.barrier, new EmptyMessage());
        }
    }

    #endregion

    #region 弱点の同期

    /// <summary>
    /// 弱点ポイント送信
    /// </summary>
    /// <param name="index">弱点ポイントのインデックス</param>
    /// <param name="color">変更カラー</param>
    public void SendWeakPoint(int index, Color color)
    {
        WeakPointData d = new WeakPointData()
        {
            index = index,
            color = color
        };

        goddess.FindWeakPoint(index).UpdateColor(color);
        goddess.AddCrack();

        if (isServer)
        {
            NetworkServer.SendToAll(MyMsg.color, d);
        }
        else
        {
            client.Send(MyMsg.color, d);
        }
    }

    void OnWeakPointReceived(NetworkMessage message)
    {
        print("WeakPointを受信しました。");
        WeakPointData d = message.ReadMessage<WeakPointData>();
        goddess.FindWeakPoint(d.index).UpdateColor(d.color);
        goddess.AddCrack();
    }

    public class WeakPointData : MessageBase
    {
        public int index;

        public Color color;
    }

    #endregion

    #region 女神像の声の同期

    /// <summary>
    /// 女神像の声の割り当てを送信
    /// </summary>
    public void SendGoddessVoice()
    {
        if (isServer)
        {
            GoddessVoiceData sd = new GoddessVoiceData
            {
                key = goddess.myKey
            };
            NetworkServer.SendToAll(MyMsg.goddessVoice, sd);
        }
        else
        {
            print("クライアントからサーバーへ、ボイス情報取得願い");
            client.Send(MyMsg.goddessVoice, new EmptyMessage());
        }
    }

    void OnGoddessReceived(NetworkMessage message)
    {

        if (isServer)
        {
            print("クライアントからボイス情報取得願いを受け取り");
            GoddessVoiceData sd = new GoddessVoiceData
            {
                key = goddess.myKey
            };
            print("サーバーから" + sd.key + "をクライアントに送る");
            NetworkServer.SendToAll(MyMsg.goddessVoice, sd);
        }
        else
        {
            GoddessVoiceData d = message.ReadMessage<GoddessVoiceData>();
            print("サーバーから" + d.key + "を受信");
            goddess.SetMyVoice(d.key);
        }
    }

    public class GoddessVoiceData : MessageBase
    {
        public int key;
    }

    #endregion

    #region 浄化

    /// <summary>
    /// 浄化アクションの送信
    /// </summary>
    public void SendPurification()
    {
        if (isServer)
        {
            print("サーバーからクライアントに浄化を伝える。");
            NetworkServer.SendToAll(MyMsg.purification, new EmptyMessage());
            goddess.Purification();
        }
        else
        {
            print("クライアントからサーバーに浄化を伝える。");
            client.Send(MyMsg.purification, new EmptyMessage());
        }
    }

    void OnPurificationReceived(NetworkMessage message)
    {
        goddess.Purification();

        //浄化はサーバーからクライアントに
        if (isServer)
        {
            NetworkServer.SendToAll(MyMsg.purification, new EmptyMessage());
        }
    }

    #endregion

    #region リスポーン

    public void SendRespawn()
    {
        RespawnData d = new RespawnData()
        {
            nextPos = goddess.transform.localPosition,
            rotY = goddess.transform.localEulerAngles.y
        };

        if (isServer)
        {
            //サーバーの場合は女神像をリスポーンした後、クライアントにアクションを送る
            goddess.Respawn(goddess.transform.position, d.rotY, Space.World);
            NetworkServer.SendToAll(MyMsg.respawn, d);
        }
    }

    void OnRespawnReceived(NetworkMessage message)
    {
        RespawnData d = message.ReadMessage<RespawnData>();
        goddess.Respawn(d.nextPos, d.rotY, Space.Self);
    }

    /// <summary>
    /// リスポーンアクションの送受信データ
    /// </summary>
    public class RespawnData : MessageBase
    {
        public Vector3 nextPos;
        public float rotY;
    }

    #endregion

    #region ゲームスタート

    void SendGameStart()
    {
        //ゲームスタートはクライアントの準備が出来た後に、サーバーに準備完了のアクションを送るため、クライアントのみ
        if (!isServer)
        {
            client.Send(MyMsg.gameStart, new EmptyMessage());
            goddess.GameStart();
        }
    }

    void OnGameStartReceived(NetworkMessage message)
    {
        //ゲームスタートはサーバーからクライアントのみ
        if (isServer)
        {
            goddess.GameStart();
        }
    }

    #endregion

    #region タイマー

    /// <summary>
    /// タイマーの送信（時間のずれをなくすために、毎秒サーバーから送信する）
    /// </summary>
    public void SendTime(int time)
    {
        TimeData d = new TimeData()
        {
            time = time
        };
        NetworkServer.SendToAll(MyMsg.time, d);
    }

    void OnTimeReceived(NetworkMessage message)
    {
        TimeData d = message.ReadMessage<TimeData>();
        goddess.SetTime(d.time);
    }

    public class TimeData : MessageBase
    {
        public int time;
    }

    #endregion

    #region デスポーン

    public void SendDespawn()
    {
        //デスポーン処理はサーバーが処理をし、クライアントにアクションを送る
        if (isServer)
        {
            goddess.DespawnMoveGoddess();
            NetworkServer.SendToAll(MyMsg.despawn, new EmptyMessage());
        }
    }

    void OnDespawnReceived(NetworkMessage message)
    {
        goddess.DespawnMoveGoddess();
    }

    #endregion

    #region 測定値の取得

    public void SendAverage()
    {
        AverageData d = new AverageData()
        {
            average = goddess.player.GetAverage()
        };

        if (isServer)
        {
            NetworkServer.SendToAll(MyMsg.average, d);
        }
        else
        {
            client.Send(MyMsg.average, d);
        }
    }

    void OnAverageReceived(NetworkMessage message)
    {
        AverageData d = message.ReadMessage<AverageData>();
        goddess.player.ShowResult(d.average);

        //クライアントのみ平均値を送る（誤差が生まれないように、サーバーで統括処理をする。）
        if (!isServer)
        {
            SendAverage();
        }
    }

    public class AverageData : MessageBase
    {
        public int average;
    }

    #endregion

}
