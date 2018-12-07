# OracleContract 

Oracle是基于Alchemint系统的一个智能合约,负责为[solution](https://github.com/Alchemint/solution)和[businessSolution](https://github.com/Alchemint/businessSolution)项目提供外部数据以及全局参数配置

## Release Note：

**1.0.1**

### Script Hash : 

* 0xfde69a7dd2a1c948977fb3ce512158987c0e2197 (PrivateNet)  
* 0xfde69a7dd2a1c948977fb3ce512158987c0e2197 (TestNet)  

SAR Contract Address: 

## 接口介绍

 方法  | 参数 | 返回值 | 描述 |
--- | --- | --- | --- 
setTypeA | byte[]=>addr、int=>value | bool | 设置(B端C端)全局config参数;设置(B端)锚定物白名单;
getTypeA | byte[]=>addr | int | 获取(B端C端)全局config参数;获取(B端)锚定物是否设置白名单;
setAccount | string=>key、byte[]=>addr | bool | 设置合约中参数
getAccount | string=>key | byte[] | 获取合约中参数
addParaAddrWhit | string=>key、byte[]=>addr、int=>value | bool | 对key添加一个授权节点Addr
removeParaAddrWhit | string=>key、byte[]=>addr | bool | 对key移除一个授权节点Addr
setTypeB | string=>key、byte[]=>addr、int=>value | bool | 设置数字资产价格($);设置锚定物对应美元汇率($)
getTypeB | string=>key | int | 获取多节点取中位数之后的数字资产和锚定物价格($)
setStructConfig | - | bool | 全局配置对象Config赋值存储
getStructConfig | - | Config | 获取全局配置对象Config
getApprovedAddrs | string=>key | object(NodeObj[]) | 根据key查询已授权喂价器地址和状态
getAddrWithParas | string=>key | object(NodeObj[]) | 根据key查询喂价器地址和价格

## Using OracleContract 

Using "getTypeA" key:你想要获取的配置名称
```C#
if (operation == "getTypeA") {

    if (args.Length != 1) return false;

    string key = (string)args[0];

    return getTypeA(key); 
    
    }
```
TypeA 方法中能获取到的所有Key名称如下:
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
Using "setTypeB" 设置单个节点从交易所获取到的价格

* para:你想要喂价的Key名
* from:喂价的节点钱包地址
* value:价格

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
Using "getTypeB" 获取所有喂价节点取中位数后的价格 key:你想要获取的Key名称 

目前所有的资产Key名 "neo_price", "sneo_price", "gas_price", "sds_price"
```C#
if (operation == "getTypeB") {

  if (args.Length != 1) return false;
 
  string key = (string)args[0];

  return getTypeB(key);
}
```
Using "getStructConfig" 获取所有TypeA中全局参数的值
```C#
if (operation == "getStructConfig") {
               
      return getStructConfig();
}
```



