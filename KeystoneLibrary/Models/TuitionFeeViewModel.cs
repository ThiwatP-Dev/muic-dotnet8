namespace KeystoneLibrary.Models
{
    public class TuitionFeeViewModel
    {
        public List<FeeCalculationItem> TermFeeItems { get; set; }
        public List<FeeCalculationItem> TuitionFeeItems { get; set; }

        public List<FeeGroupItem> FeeGroupItems
        {
            get
            {
                var feeGroupItems = new List<FeeGroupItem>();
                if (TermFeeItems != null)
                {
                    feeGroupItems.AddRange(TermFeeItems.GroupBy(x => x.Name)
                                                       .Select(x => new FeeGroupItem
                                                                    {
                                                                        Name = x.Key,
                                                                        Amount = x.Sum(y => y.MultiplyAmount)
                                                                    })
                                                       .ToList());
                }

                if (TuitionFeeItems != null)
                {
                    feeGroupItems.AddRange(TuitionFeeItems.GroupBy(x => x.Name)
                                                          .Select(x => new FeeGroupItem
                                                                       {
                                                                           Name = x.Key,
                                                                           Amount = x.Sum(y => y.MultiplyAmount)
                                                                       }).ToList());
                }
                return feeGroupItems;
            }
        }

        public decimal Total => FeeGroupItems.Sum(x => x.Amount);
    }

    public class FeeCalculationItem
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public string Multiply { get; set; }
        public int PaymentCredit { get; set; }
        public decimal MultiplyAmount => Amount * (Multiply == "m" ? PaymentCredit : 1);
    }

    public class FeeGroupItem
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
    }
}