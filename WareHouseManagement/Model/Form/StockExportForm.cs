﻿using NanoidDotNet;
using WareHouseManagement.Model.Receipt;

namespace WareHouseManagement.Model.Form
{
    public class StockExportForm: FormGeneric
    {
        public StockExportForm()
        {
            Id= $"XUATKHO-{Nanoid.Generate(Nanoid.Alphabets.LowercaseLettersAndDigits, 5)}";
        }
        public DateTime OrderDate { get; set; }
        public string ReceiptId { get; set; }
        public virtual CustomerBuyReceipt Receipt { get; set; }
    }
}
