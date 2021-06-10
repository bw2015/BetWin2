<?php 
      $ddh = $_POST['ddh']; //支付宝订单号
       
      $key = $_POST['key']; //KEY验证
       
      $PayMore = $_POST['PayMore']; //备注信息
       
      $pay = $_POST['pay']; //分类 =1 支付宝 =2财付通 =3 微信
       
        $PayJe = $_POST['PayJe'];//金额
         
      $appid = $_POST['appid'];//网址APPID
       
if($key=="40042114"){
 
   echo "执行您的接口并写入数据库";
    
}
?>