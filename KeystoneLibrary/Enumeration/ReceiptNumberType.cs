namespace KeystoneLibrary.Enumeration
{
    public enum ReceiptNumberType
    {
        REGISTRATION,
        OTHER
    }

    public static class ReceiptNumberTypeExtension
    {
        public static string ToStringValue(this ReceiptNumberType receiptNumberType)
        {
            string value = string.Empty;
            switch (receiptNumberType)
            {
                case ReceiptNumberType.REGISTRATION:
                    value = "REG";
                    break;
                case ReceiptNumberType.OTHER:
                    value = "RC";
                    break;
                default:
                    value = "RC";
                    break;
            }
            return value;
        }
    }
}