Imports System.Net
Imports System.IO
Imports System.Text

Public Class huawei_mobile_wifi_sms_api
    Shared Function SendSMS(url As String, username As String, password As String, number As String, content As String) As Integer
        'URL必须以特定格式，比如http://192.168.8.1，不能有多余的/
        '谨防XML注入
        Dim LoginData As String = HttpPost(url & "/api/user/login", "<?xml version:""1.0"" encoding=""UTF-8""?><request><Username>" & username & "</Username><Password>" & System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password)) & "</Password></request>")
        If (LoginData.Contains("OK")) Then
        Else
            Return -1 '用户名或密码不正确
        End If
        '<SucPhone>10001</SucPhone>
        Dim SendData As String = HttpPost(url & "/api/sms/send-sms", "<?xml version:""1.0"" encoding=""UTF-8""?><request><Index>-1</Index><Phones><Phone>" & number & "</Phone></Phones><Sca></Sca><Content>" & content & "</Content><Length>" & content.Length & "</Length><Reserved>1</Reserved><Date>" & Format(Now(), "yyyy-MM-dd HH:mm:ss") & "</Date></request>")
        If (SendData.Contains("OK")) Then
        Else
            Return -2 '发送短信失败（多为设备原因）
        End If
        Dim t As UInteger = 0
        Do
            If (t > 100) Then
                Return -5 'Timeout
            End If
            t += 1
            '等待短信发送完成
            Dim StatusData As String = HttpGet(url & "/api/sms/send-status")
            If (StatusData.Contains("<SucPhone>" & number & "</SucPhone>")) Then
                Return 1 '发送成功
            End If
            If (StatusData.Contains("<FailPhone>" & number & "</FailPhone>")) Then
                Return -3 '发送短信失败（多为运营商原因）
            End If
            System.Threading.Thread.Sleep(200)
        Loop
        Return 0 '未知错误
    End Function
    Shared Function HttpPost(URL As String, PostData As String) As String
        Dim request As WebRequest = WebRequest.Create(URL)
        request.Method = "POST"
        Dim byteArray As Byte() = Encoding.UTF8.GetBytes(PostData)
        request.ContentType = "application/x-www-form-urlencoded"
        request.ContentLength = byteArray.Length
        Dim dataStream As Stream = request.GetRequestStream()
        dataStream.Write(byteArray, 0, byteArray.Length)
        dataStream.Close()
        Dim response As WebResponse = request.GetResponse()
        If (CType(response, HttpWebResponse).StatusCode = HttpStatusCode.OK) Then
            dataStream = response.GetResponseStream()
            Dim reader As New StreamReader(dataStream)
            Dim responseFromServer As String = reader.ReadToEnd()
            reader.Close()
            dataStream.Close()
            response.Close()
            Return responseFromServer
        Else
            Return ""
        End If
    End Function
    Shared Function HttpGet(URL As String) As String
        Dim request As WebRequest = WebRequest.Create(URL)
        request.Credentials = CredentialCache.DefaultCredentials
        Dim response As WebResponse = request.GetResponse()
        If CType(response, HttpWebResponse).StatusCode = HttpStatusCode.OK Then
            Dim dataStream As Stream = response.GetResponseStream()
            Dim reader As New StreamReader(dataStream)
            Dim responseFromServer As String = reader.ReadToEnd()
            reader.Close()
            response.Close()
            Return responseFromServer
        Else
            Return ""
        End If
    End Function
End Class
