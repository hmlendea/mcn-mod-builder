using System.Collections.Generic;
using System.Linq;

using MoreCulturalNamesModBuilder.DataAccess.DataObjects;
using MoreCulturalNamesModBuilder.Service.Models;

namespace MoreCulturalNamesModBuilder.Service.Mapping
{
    static class NameMapping
    {
        internal static Name ToServiceModel(this NameEntity dataObject)
        {
            Name serviceModel = new Name();
            serviceModel.LanguageId = dataObject.LanguageId;
            serviceModel.Value = dataObject.Value;

            return serviceModel;
        }

        internal static NameEntity ToDataObject(this Name serviceModel)
        {
            NameEntity dataObject = new NameEntity();
            dataObject.LanguageId = serviceModel.LanguageId;
            dataObject.Value = serviceModel.Value;

            return dataObject;
        }

        internal static IEnumerable<Name> ToServiceModels(this IEnumerable<NameEntity> dataObjects)
        {
            IEnumerable<Name> serviceModels = dataObjects.Select(dataObject => dataObject.ToServiceModel());

            return serviceModels;
        }

        internal static IEnumerable<NameEntity> ToDataObjects(this IEnumerable<Name> serviceModels)
        {
            IEnumerable<NameEntity> dataObjects = serviceModels.Select(serviceModel => serviceModel.ToDataObject());

            return dataObjects;
        }
    }
}
