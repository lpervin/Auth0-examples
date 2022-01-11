using CFSWeb.Model.Accounting.Entities;
using CFSWeb.Model.Accounting.Indy;
using CFSWeb.Model.Accounting.Insourcing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFSWeb.Data.Extensions
{
    public static class FundingBatchesExtensions
    {

        public static BatchGroupedByDealer ToARCsPaymentVoucher(this IndyPaymentVouchersByRemitCode indyPaymentVoucherRemits)
        {
            return new BatchGroupedByDealer
            {
                Amount = indyPaymentVoucherRemits.TotalTreasuriesAmount.ToString(),
                BatchId = indyPaymentVoucherRemits.BatchNumber.ToString(),
                Count = indyPaymentVoucherRemits.TreasuriesInVoucherCnt.ToString(),
                DealerName = indyPaymentVoucherRemits.TreasuryVendorName,
                DealerNumber = indyPaymentVoucherRemits.RemitCode.Replace("-E", "").Replace("-S", ""),
                FundingFrequency = indyPaymentVoucherRemits.TreasuryVendorFundingFrequency,
                FundType = string.IsNullOrEmpty(indyPaymentVoucherRemits.PaymentVoucherRemittanceType) ? string.Empty : indyPaymentVoucherRemits.PaymentVoucherRemittanceType.Substring(0, 1), 
                CreateDate = DateTime.Now.ToString()

            };
           
        }

        public static BatchCsa ToBatchCsa(this CSAFundingBatch csaBatch)
        {
            return new BatchCsa { 
                BatchNumber = csaBatch.BatchNumber,
                dealerRemitAmt = csaBatch.RemitToCsaBatchAmount == null ? 0 : csaBatch.RemitToCsaBatchAmount.Value,
                fromDate = csaBatch.BusinessActivityFromDate.Value,
                toDate = csaBatch.BusinessActivityToDate.Value                
            };
        }
    }
}
