using System;
using System.ComponentModel;

namespace Admin.Models
{
    public enum EProvisioningTask
    {
        [Description("New provisioning based on model, optionally deleting old provisioning.")]
        Create,

        [Description("Update existing provisioning read from the database with updated information from the model.")]
        Update,

        [Description("Remove provisioning elements from the database.")]
        Delete,

        [Description("Verify integrity and consistency of provisioning metadata.")]
        Verify,

        [Description("Delete + Create"), Obsolete("Nur wegen Rückwärtskompatibilität mit Admin-Oberfläche")]
        Rebuild,

        [Description("Export filter relationships to .xgml format")]
        Visualize
    }
}