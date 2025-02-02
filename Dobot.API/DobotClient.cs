﻿using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Dobot.API
{
  public abstract class DobotClient
  {
    private Socket mSocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    public string IP { get; private set; }
    public int Port { get; private set; }

    /// <summary>
    ///   连接设备
    /// </summary>
    /// <param name="strIp">设备地址</param>
    /// <param name="iPort">指定端口</param>
    /// <returns>true成功，false失败</returns>
    public bool Connect(string strIp, int iPort)
    {
      var bOk = false;
      try
      {
        IP = strIp;
        Port = iPort;

        var addr = IPAddress.Parse(strIp);
        var endpt = new IPEndPoint(addr, iPort);

        mSocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        mSocketClient.Connect(endpt);
        mSocketClient.SendTimeout = 5000;
        mSocketClient.ReceiveTimeout = 5000;

        OnConnected(mSocketClient);

        bOk = true;
      }
      catch (Exception ex)
      {
        Debug.WriteLine("Connect failed:" + ex);
      }

      return bOk;
    }

    /// <summary>
    ///   断开连接
    /// </summary>
    public void Disconnect()
    {
      if (mSocketClient.Connected)
        try
        {
          mSocketClient.Shutdown(SocketShutdown.Both);
          mSocketClient.Close();
        }
        catch (Exception ex)
        {
          Debug.WriteLine("close socket:" + ex);
        }

      OnDisconnected();
    }

    public bool IsConnected()
    {
      try
      {
        return mSocketClient.Connected;
      }
      catch (Exception ex)
      {
      }

      return false;
    }

    protected abstract void OnConnected(Socket sock);
    protected abstract void OnDisconnected();


    /// <summary>
    ///   发送数据
    /// </summary>
    /// <param name="str">发送内容</param>
    /// <returns>成功-true，失败-false</returns>
    protected bool SendData(string str)
    {
      try
      {
        var data = Encoding.UTF8.GetBytes(str);
        return mSocketClient.Send(data) == data.Length ? true : false;
      }
      catch (Exception ex)
      {
        Debug.WriteLine("send error:" + ex);
      }

      return false;
    }

    /// <summary>
    ///   等待响应
    /// </summary>
    /// <param name="iTimeoutMillsecond">等待时间，毫秒单位</param>
    /// <returns>返回响应的内容</returns>
    protected string WaitReply(int iTimeoutMillsecond)
    {
      try
      {
        if (iTimeoutMillsecond != mSocketClient.ReceiveTimeout) mSocketClient.ReceiveTimeout = iTimeoutMillsecond;
        var buffer = new byte[1024];
        var length = mSocketClient.Receive(buffer);
        var str = Encoding.UTF8.GetString(buffer, 0, length);

        return str;
      }
      catch (Exception ex)
      {
        Debug.WriteLine("send error:" + ex);
        return "send error:" + ex.Message;
      }
    }

    protected int Receive(byte[] buffer, int offset, int size, SocketFlags flag)
    {
      return mSocketClient.Receive(buffer, offset, size, flag);
    }
  }
}