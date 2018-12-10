# OracleContract 

Oracle is a smart contract based on the Alchemint system that provides external data and global parameter configuration for [solution](https://github.com/Alchemint/solution) and [businessSolution](https://github.com/Alchemint/businessSolution) projects.

## Release Note：

**1.0.1**

### Script Hash : 

* 0xfde69a7dd2a1c948977fb3ce512158987c0e2197 (PrivateNet)  
* 0xfde69a7dd2a1c948977fb3ce512158987c0e2197 (TestNet)  

SAR Contract Address: 

## Interface introduction

 Method  | Parameter  | Return Value | Description |
--- | --- | --- | --- 
setTypeA | byte[]=>addr、int=>value | bool | Set (B-side C-side) global config parameters; set (B-side) anchor whitelist;
getTypeA | byte[]=>addr | int | Get (B-side C-side) global config parameter; get (B-side) anchor to set whitelist;
setAccount | string=>key、byte[]=>addr | bool | Set the parameters in the contract
getAccount | string=>key | byte[] | Get the parameters in the contract
addParaAddrWhit | string=>key、byte[]=>addr、int=>value | bool | Add an authorization node Addr to the key
removeParaAddrWhit | string=>key、byte[]=>addr | bool | Remove an authorized node from the key
setTypeB | string=>key、byte[]=>addr、int=>value | bool | Set the digital asset price ($); set the exchange rate anchored to the US dollar ($) assignment storage
getTypeB | string=>key | int | Get the median price($) of digital asset and anchor from multi-nodes 
setStructConfig | - | bool | Global configuration object Config assignment storage
getStructConfig | - | Config | Get the global configuration object Config
getApprovedAddrs | string=>key | object(NodeObj[]) | Query the authorized feeder address and status according to the key
getAddrWithParas | string=>key | object(NodeObj[]) | Query the feeder address and price according to the key
## Using OracleContract 

Using "getTypeA" key: the name of the configuration you want to get
```C#
if (operation == "getTypeA") {

    if (args.Length != 1) return false;

    string key = (string)args[0];

    return getTypeA(key); 
    
    }
```
All Key names that can be obtained in the TypeA method are as follows:
```C#
  public BigInteger liquidate_line_rate_b;

  public BigInteger liquidate_line_rate_c;

  public BigInteger liquidate_dis_rate_c;

  public BigInteger fee_rate_c;

  public BigInteger liquidate_top_rate_c;

  public BigInteger liquidate_line_rateT_c;

  public BigInteger issuing_fee_c;

  public BigInteger issuing_fee_b;

  public BigInteger debt_top_c;
```
Using "setTypeB" sets the price that a single node gets from the exchange

* Para: the name of the key you want to feed
* From: the node wallet address of the feed price
* Value: price

```C#
 if (operation == "setTypeB")  {
  
  if (args.Length != 3) return false;

  string para = (string)args[0];

  byte[] from = (byte[])args[1];

  BigInteger value = (BigInteger)args[2];

  BigInteger state = (BigInteger)Storage.Get(Storage.CurrentContext, GetParaAddrKey(para, from)).AsBigInteger();

  if (state == 0) return false;

  return setTypeB(para, from, value);
}
```
Using "getTypeB" to get the price of all the feed nodes after taking the median key: the name of the key you want to get

Currently the Key name of all asset names: "neo_price", "sneo_price", "gas_price", "sds_price"
```C#
if (operation == "getTypeB") {

  if (args.Length != 1) return false;
 
  string key = (string)args[0];

  return getTypeB(key);
}
```
Using "getStructConfig" to get the values of all global parameters in TypeA
```C#
if (operation == "getStructConfig") {
               
      return getStructConfig();
}
```



