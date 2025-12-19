using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comax.Common.DTOs.Rating
{
    public class RatingRequest
    {
        public int ComicId { get; set; }
        public int Score { get; set; }
    }
}
