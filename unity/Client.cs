using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;


/// <summary>
/// Client class shows how to implement and use TcpClient in Unity.
/// </summary>
public class Client : MonoBehaviour
{
    public float waitingMessagesFrequency = 1;
    [SerializeField]
    public int port = 8008;
    Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    // Get DNS host information.
    [TextArea(15,20)]
    public string receivedMessage;

    bool haveHeader;
    int dataReceived;
    int dataRemaining;
    int packet_size;
    static int BUFSIZE = 2048;
    int HEADSIZE = 4;
    byte[] recvbuf = new byte[BUFSIZE];

    public void Start()
    {
        haveHeader = false;
        dataReceived = 0;
        OpenClient();
        if (sender.Connected) {
            SendMessage("Send me some data.<EOF>");
        }
    }

    public void Update()
    {
        if (sender.Connected) {
            ReceiveMessage();
        }
    }


    public void OpenClient()
    {
        try
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = ipHostInfo.AddressList[1];
            Debug.Log(string.Format(ipAddress.ToString()));
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
            // Connect the socket to the remote endpoint. Catch any errors.  
            try
            {
                sender.Connect(remoteEP);

                Debug.Log(string.Format("Socket connected to {0}",
                    sender.RemoteEndPoint.ToString()));
            }
            catch (ArgumentNullException ane)
            {
                Debug.Log(string.Format("ArgumentNullException : {0}", ane.ToString()));
                CloseClient();
            }
            catch (SocketException se)
            {
                Debug.Log(string.Format("SocketException : {0}", se.ToString()));
                CloseClient();
            }
            catch (Exception e)
            {
                Debug.Log(string.Format("Unexpected exception : {0}", e.ToString()));
                CloseClient();
            }

        }
        catch (Exception e)
        {
            Debug.Log(string.Format(e.ToString()));
            CloseClient();
        }
    }

    public void SendMessage(string _msg)
    {
        // Encode the data string into a byte array.  
        byte[] sendbuf = Encoding.Default.GetBytes(_msg);
        try
        {
            // Send the data through the socket.  
            int bytesSent = sender.Send(sendbuf);
        }
        catch (ArgumentNullException ane)
        {
            Debug.Log(string.Format("ArgumentNullException : {0}", ane.ToString()));
            CloseClient();
        }
        catch (SocketException se)
        {
            Debug.Log(string.Format("SocketException : {0}", se.ToString()));
            CloseClient();
        }
        catch (Exception e)
        {
            Debug.Log(string.Format("Unexpected exception : {0}", e.ToString()));
            CloseClient();
        }

    }
    public void ReceiveMessage()
    {
        try
        {
            if (Input.GetKeyDown("space"))
            {
                CloseClient();
            }
            if (!haveHeader)
            {
                // Ask for header
                byte[] headbuf = new byte[HEADSIZE];
                int headRec = sender.Receive(headbuf);
                if (headRec <= 0)
                {
                    Debug.Log("Connection closed by remote host or exception thrown.\n");
                    CloseClient();
                }
                packet_size = BitConverter.ToInt32(headbuf, 0);
                Debug.Log("Packet Size: " + packet_size + "\n");
                dataRemaining = packet_size;
                dataReceived = 0;
                haveHeader = true;
                Array.Clear(recvbuf, 0, recvbuf.Length);
            }

            
            if (dataReceived < packet_size) {
                Debug.Log("DataReceived smaller, Packet size:" + packet_size);
                //ask for data
                int readRet = sender.Receive(recvbuf, dataReceived, dataRemaining, SocketFlags.None);
                if (readRet <= 0)
                {
                    Debug.Log("Connection closed by remote host or exception thrown.\n");
                    CloseClient();
                }
                dataReceived += readRet;
                dataRemaining -= readRet;



                if (dataReceived == packet_size)
                {
                    Debug.Log("DataReceived equal, Packet size:" + packet_size);
                    haveHeader = false;
                    receivedMessage = Encoding.Default.GetString(recvbuf);
                }
                else if (dataReceived > packet_size)
                {
                    Debug.Log("More than asked for");
                }
                //    byte[] packet = new byte[packet_size];
                //    Buffer.BlockCopy(recvbuf, 0, packet, 0, packet_size);
                //    receivedMessage = Encoding.Default.GetString(recvbuf);
                //    Debug.Log(receivedMessage);


                //    // new header is contained in the received data surplus
                //    byte[] surplusHead = new byte[HEADSIZE];
                //    // Take the surplus of received bytes
                //    Buffer.BlockCopy(recvbuf, packet_size, surplusHead, 0, surplusHead.Length);
                //    // Get new packet size
                //    packet_size = BitConverter.ToInt32(surplusHead, 0);
                //    dataRemaining = packet_size; // reset new dataRemaining
                //    Debug.Log("Packet Size from surplus: " + packet_size + "\n");

                //    int surpluslen = (dataReceived - packet_size - surplusHead.Length);
                //    byte[] surplusbuf = new byte[surpluslen];

                //    // Copy surplus data
                //    Buffer.BlockCopy(recvbuf, packet_size + HEADSIZE, surplusbuf, 0, surplusbuf.Length);

                //    // Clear Receive buffer
                //    Array.Clear(recvbuf, 0, recvbuf.Length);

                //    // Copy surplus into clean Receive buffer
                //    Buffer.BlockCopy(surplusbuf, 0, recvbuf, 0, surplusbuf.Length);

                //    dataReceived = 0;
                //    haveHeader = true;
                //}
            }

        }
        catch (ArgumentNullException ane)
        {
            Debug.Log(string.Format("ArgumentNullException : {0}", ane.ToString()));
            CloseClient();
        }
        catch (SocketException se)
        {
            Debug.Log(string.Format("SocketException : {0}", se.ToString()));
            CloseClient();
        }
        catch (Exception e)
        {
            Debug.Log(string.Format("Unexpected exception : {0}", e.ToString()));
            CloseClient();
        }

    }
    public void CloseClient()
    {
        // Release the socket.  
        sender.Shutdown(SocketShutdown.Both);
        sender.Close();
    }
}