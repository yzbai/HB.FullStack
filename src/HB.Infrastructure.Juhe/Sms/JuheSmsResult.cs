namespace HB.Infrastructure.Juhe.Sms
{
    public class JuheSmsResult
    {
        public string Reason { get; set; }
        public long Error_Code { get; set; }
        public JuheSmsResultDetail Result { get; set; }
    }

    public class JuheSmsResultDetail
    {
        public long Count { get; set; }
        public long Fee { get; set; }
        public long SId { get; set; }
    }
}


///****失败示例**/
//{
//    "reason": "错误的短信模板ID,请通过后台确认!!!",
//    "result": [],
//    "error_code": 205402
//}

///****成功示例**/
//{
//    "reason": "短信发送成功",
//    "result": {
//        "count": 1, /*发送数量*/
//        "fee": 1, /*扣除条数*/
//        "sid": 2029865577 /*短信ID*/
//    },
//    "error_code": 0 /*发送成功*/
//}