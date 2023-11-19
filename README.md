This is a proof of concept for reducing Kubernetes running cost.

It is implemeted as follows:
* a custom resource is defined, which allows to specify desired configuration
* a control-plane workload periodically checks for exprired namespaces and deletes one that have been expired
