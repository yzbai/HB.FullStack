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
}
