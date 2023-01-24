using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnvelopeASP.Models
{
    public class TransferModel
    {
        public int? DestinationNumber { get; set; }
        public List<SelectListItem> Envelopes { get; set; }

        public double Amount { get; set; }

        public TransferModel() {
            Envelopes = new List<SelectListItem>();
        }
    }
}
