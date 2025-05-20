Biatec Identity Gateway is the secure way to store the sensitive data. It uses the DeRec alliance cryptography primitives such as the encryption, signing, shamir and merkle trees.

[https://www.biatec.io/](https://www.biatec.io/)

[https://identity.biatec.io/](https://identity.biatec.io/)

[https://github.com/derecalliance/cryptography/](https://github.com/derecalliance/cryptography/)

## To generate generated clients run

```
cd BiatecIdentityGateway/Generated
docker run --rm -v ".:/app/out" scholtz2/dotnet-avm-generated-client:latest dotnet client-generator.dll --namespace "BiatecIdentityGateway.Generated" --url https://raw.githubusercontent.com/scholtz/BiatecCLAMM/refs/heads/main/contracts/artifacts/BiatecConfigProvider.arc56.json
docker run --rm -v ".:/app/out" scholtz2/dotnet-avm-generated-client:latest dotnet client-generator.dll --namespace "BiatecIdentityGateway.Generated" --url https://raw.githubusercontent.com/scholtz/BiatecCLAMM/refs/heads/main/contracts/artifacts/BiatecIdentityProvider.arc56.json
```

## Local debugging

To run cryptography primitives run 

```
docker run -p 50051:50051 scholtz2/derec-crypto-core-grpc
```