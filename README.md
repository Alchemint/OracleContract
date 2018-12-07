# OracleContract 
B端和C端公共Oracle合约

## Release Note：

**1.0.0**

Script Hash : 

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

## NodeObj

            //授权的节点地址
            public byte[] addr; 
            
            //价格/状态
            public BigInteger value;

## Config
        //B端抵押率   50
        public BigInteger liquidate_line_rate_b;
        //C端抵押率  150
        public BigInteger liquidate_line_rate_c;

        //C端清算折扣  90
        public BigInteger liquidate_dis_rate_c;

        //C端费用率  15秒的费率 乘以10的16次方  66,666,666
        public BigInteger fee_rate_c;

        //C端最高可清算抵押率  160
        public BigInteger liquidate_top_rate_c;

        //C端伺机者可清算抵押率 120
        public BigInteger liquidate_line_rateT_c;

        //C端发行费用 1000
        public BigInteger issuing_fee_c;

        //B端发行费用  1000000000
        public BigInteger issuing_fee_b;

        //C端最大发行量(债务上限)  1000000000000
        public BigInteger debt_top_c;
