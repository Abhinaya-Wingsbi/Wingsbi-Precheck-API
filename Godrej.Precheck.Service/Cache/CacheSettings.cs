using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Service.Cache
{
    
        public static class CacheSettings
        {
            // Drawing number cache settings
            public static string DrawingNumbersCacheKey = "DrawingNumbers";
            public static readonly TimeSpan DrawingNumbersCacheDuration = TimeSpan.FromMinutes(1);

            // Production number settings
            public static string ProductionSeriesCacheKey = "ProductionSeries";
            public static readonly TimeSpan ProductionSeriesCacheDuration = TimeSpan.FromMilliseconds(1);

            // Precheck Modules cache settings
            public static string PrecheckModulesCacheKey = "PrecheckModules";
            public static readonly TimeSpan PrecheckModulesCacheDuration = TimeSpan.FromMilliseconds(1); 

            // Component Types cache settings
            public static string ComponentTypesCacheKey = "ComponentTypes";
            public static readonly TimeSpan ComponentTypesCacheDuration = TimeSpan.FromHours(2);

            // Sop Assembly  cache settings

            public static string AssemblyCacheKey = "SopAssembly";
            public static readonly TimeSpan AssemblyCacheDuration = TimeSpan.FromHours(2); // Adjust duration as needed

            // Document type cache settings
            public static string DocumentTypesCacheKey = "DocumentType";
            public static readonly TimeSpan DocumentTypesCacheDuration = TimeSpan.FromHours(2); // Adjust duration as needed


        // Unit type cache settings
        public const string UnitsCacheKey = "Units";
            public static readonly TimeSpan UnitsCacheDuration = TimeSpan.FromHours(2);

            // Shapes cache settings
            public const string ShapesCacheKey = "Shapes";
            public static readonly TimeSpan ShapesCacheDuration = TimeSpan.FromMinutes(2);

            // LnItemCode cache settings
            public const string LnItemCodesCacheKey = "LnItemCodes";
            public static readonly TimeSpan LnItemCodesCacheDuration = TimeSpan.FromMinutes(2);
    }

    
}
