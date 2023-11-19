This is a proof of concept for reducing Kubernetes running cost.

It is implemented as follows:
* a custom resource is defined, which allows specifying desired configuration
* a control-plane workload periodically checks for expired namespaces and deletes ones that have been expired
