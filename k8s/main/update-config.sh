kubectl apply -f deployment.yaml -n biatec-identity
kubectl delete configmap biatec-identity-gateway-conf -n biatec-identity
kubectl create configmap biatec-identity-gateway-conf --from-file=conf -n biatec-identity
kubectl rollout restart deployment/biatec-identity-gateway-app-deployment -n biatec-identity
kubectl rollout status deployment/biatec-identity-gateway-app-deployment -n biatec-identity
