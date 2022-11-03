**Mac上安装和运行mongodb流程**
//需要工具 cmd studio3T
1.cd /usr/local
2.sudo curl -O https://fastdl.mongodb.org/osx/mongodb-osx-ssl-x86_64-4.0.9.tgz (下载操作 版本可以自选)
3.sudo tar -zxvf mongodb-osx-ssl-x86_64-4.0.9.tgz （解压）
4.sudo mv mongodb-osx-x86_64-4.0.9/ mongodb (重命名mongo文件夹名称)
5.export PATH=/usr/local/mongodb/bin:$PATH (安装完成后，我们可以把 MongoDB 的二进制命令文件目录（安装目录/bin）添加到 PATH 路径中)


创建日志及数据存放的目录：
数据存放路径：
sudo mkdir -p /usr/local/var/mongodb
日志文件路径：
sudo mkdir -p /usr/local/var/log/mongodb
接下来要确保当前用户对以上两个目录有读写的权限：
sudo chown runoob /usr/local/var/mongodb
sudo chown runoob /usr/local/var/log/mongodb

启动mongo：
sudo mongod  --config  /usr/local/etc/mongod.conf

之后就可以使用Studio 3T连接mongo数据库了 默认localhost 27017

*如果使用启动cmd 提示mongod无效可以先使用第5步 export命令。*