namespace HB.Infrastructure.Tencent.TCaptha
{
    public class TCaptchaResult
    {
        public int Ret { get; set; } = -1;

        public string Ticket { get; set; } = null!;

        public string AppId { get; set; } = null!;

        public string Randstr { get; set; } = null!;

        public bool IsSuccessed { get => Ret == 0; }
    }

    /*
     
    "{\"appid\":\"2029147713\",\"ret\":0,\"ticket\":\"t03XIE9WJnOdOn_rWFgyiTS6eflv_gKaHbT8lg3CCYBZXYPiHlNBaRYvu8W11Nj1aRVaTrGiC_q0XaSGUed-hSjov_-8wWc7P4QpsZG8rSrtENU_Xrd-f5mV8Gr8ChN164y3lO40FIhd5duugwqfUcShwo0_D3ornHrWygSZ9guYo89gYMsXF8f9A**\",\"randstr\":\"@Q7G\"}"
     
     */
}
