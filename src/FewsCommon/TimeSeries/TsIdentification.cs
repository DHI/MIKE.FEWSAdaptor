using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FewsCommon.TimeSeries
{
    /// <summary>
    /// Structure containing TS metadata. Used for selection...
    /// </summary>
    public class TsIdentification
    {
        private string _locationId = string.Empty;
        private string parmId = "";
        private string _tsType = "";
        private string _ensembleId = "";
        private string _ensembleMemberId = "";
        private string _itemName = "";

        /// 
        /// <param name="locationId">LocationId</param>
        /// <param name="parmId">ParametrId (ItemInfo.Quantity.Item + ItemInfo.Quantity.
        /// Unit in MZ))</param>
        /// <param name="tsType">TS type. Default instantaneous.</param>
        /// <param name="ensembleId">EnsembleId</param>
        /// <param name="ensembleMemberId">Ensemble MemberId</param>
        public TsIdentification(string locationId, string parmId, string tsType, string ensembleId, string ensembleMemberId)
        {
            _locationId = locationId ?? throw new ArgumentNullException(nameof(locationId));
            this.parmId = parmId ?? throw new ArgumentNullException(nameof(parmId));
            this._tsType = tsType ?? throw new ArgumentNullException(nameof(tsType));
            this._ensembleId = ensembleId ?? throw new ArgumentNullException(nameof(ensembleId));
            this._ensembleMemberId = ensembleMemberId ?? throw new ArgumentNullException(nameof(ensembleMemberId));
        }

        public TsIdentification(TimeSeriesNS.TimeSeries ts, string itemName)
        {
            LocationId = ts.LocationId;
            ParmId = ts.ParameterId;
            TsType = ts.Type;
            EnsembleId = ts.EnsembleId;
            EnsembleMemberId = ts.EnsembleMemberId;
            ItemName = itemName;
        }

        /// <summary>
        /// LocationId
        /// </summary>
        public string LocationId { get => _locationId; set => _locationId = value; }
        /// <summary>
        /// ParametrId (ItemInfo.Quantity.Item + ItemInfo.Quantity.Unit in MZ))
        /// </summary>
        public string ParmId { get => parmId; set => parmId = value; }
        /// <summary>
        /// TS type. Default instantaneous.
        /// </summary>
        public string TsType { get => _tsType; set => _tsType = value; }
        /// <summary>
        /// EnsembleId
        /// </summary>
        public string EnsembleId { get => _ensembleId; set => _ensembleId = value; }
        /// <summary>
        /// EnsembleMemberId
        /// </summary>
        public string EnsembleMemberId { get => _ensembleMemberId; set => _ensembleMemberId = value; }
        public string ItemName { get => _itemName; set => _itemName = value; }
    }
}
