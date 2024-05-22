/home/odoo/Desktop/Dev/C#/ERP/lib/model/CachedModel.cs


=> GetCachedModelOfTargetField()
==> Don't forget to update the method to support doted operator


For a link between M2O <=> O2M, if one of both is in ToRecompute=True, the other one also needs to be in ToRecompute=True

Check if it's possible to override the "=" operator (and getter)

Model should override System.Collections.ICollection, as it could be a collection
