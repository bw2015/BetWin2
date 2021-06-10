#!/bin/bash
############ 一键安装 Kangle 脚本 ##############
#Author:Louis
#Update:2018-10-27
##################### End ######################
dir='/'
yum -y install wget make automake gcc gcc-c++ pcre-devel zlib-devel sqlite-devel openssl-devel vim
yum -y install lrzsz
#安装Kangle

cd ${dir}
wget http://a8.to/download/kangle.tar.gz
tar zxvf kangle.tar.gz
cd kangle-*
./configure --prefix=/vhs/kangle --enable-disk-cache --enable-ipv6 --enable-ssl --enable-vh-limit
make
make install  

#替换端口号
#sed -i 's/3311/44333/g;s/10M/512M/g;9s/10/3600/g;' /vhs/kangle/etc/config.xml

curl https://a8.to/download/config.xml > /vhs/kangle/etc/config.xml

#启动脚本
/vhs/kangle/bin/kangle

#添加开机启动脚本
cd /etc/init.d/
cat>kangle.sh<<EOF
#!/bin/bash
#this a Application script
#add for chkconfig
#chkconfig: 2345 70 30
/vhs/kangle/bin/kangle
EOF

#给kangle.sh添加执行权限
chmod +x kangle.sh
chkconfig kangle.sh on

cd /etc/
cat>hosts<<EOF
10.144.116.56	betwin
EOF

firewall-cmd --zone=public --add-port=44333/tcp --permanent
firewall-cmd --reload

cd ${dir}


