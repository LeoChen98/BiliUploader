# BiliUploader
B站投稿工具（命令行）

**2019年6月11日测试可用**


[![Version](https://img.shields.io/github/release/LeoChen98/BiliUploader.svg?label=Version)](https://github.com/LeoChen98/BiliUploader/releases)
[![GitHub issues](https://img.shields.io/github/issues/LeoChen98/BiliUploader.svg)](https://github.com/LeoChen98/BiliUploader/issues)
[![需要帮助的 issue](https://img.shields.io/github/issues/LeoChen98/BiliUploader/help%20wanted.svg?label=需要帮助的%20issue)](https://github.com/LeoChen98/BiliUploader/issues?q=is%3Aissue+is%3Aopen+label%3A%22help+wanted%22)
[![Language](https://img.shields.io/badge/%E8%AF%AD%E8%A8%80-%E4%B8%AD%E6%96%87-brightgreen.svg)](#)
[![DevLanguage](https://img.shields.io/badge/%E5%BC%80%E5%8F%91%E8%AF%AD%E8%A8%80-C%23-brightgreen.svg)](#)
[![.netVersion](https://img.shields.io/badge/.net-4.5-brightgreen.svg)](#)
[![Pull Request Welcome](https://img.shields.io/badge/Pull%20request-welcome-brightgreen.svg)](#)
[![GitHub license](https://img.shields.io/github/license/LeoChen98/BiliUploader.svg)](https://github.com/LeoChen98/BiliUploader/blob/master/LICENSE)

## 简述
* 本程序是用于实现B站投稿的命令行程序


## 安装和使用
* 本软件无需安装，下载解压后即可使用。
* 在不使用压制功能的情况下无需下载ffmpeg。
```
用法: BiliUploader.exe -c cookies -ls file1 [file2 file3 ...] -le -title title -type typeid -tags tags [-cover cover] [-desc description] [-dynamic dynamic] [-dt publish_time] [-copyright copyright] [-mid mission_id] [-subtitle subtitle_language] [-com compress_settings] [-f]

解析：
-c cookies                      账号cookies字符串
-ls                             视频文件列表开始标记，与-le联用，列表中文件之间用空格隔开
-le                             视频文件列表开始标记，与-ls联用
-title title                    投稿标题
-type typeid                    投稿分区
-tags tags                      投稿标签，用英文逗号隔开（","）
-cover cover                    投稿封面，默认为p1的第一张系统截图
-desc description               投稿简介
-dynamic dynamic                粉丝动态
-dt publish_time                发布时间，默认为不定时
-copyright copyright            版权，默认为1，即自制
-mid mission_id                 活动id，默认为空
-subtitle subtitle_language     字幕语言，默认为关闭字幕
-com compress_settings_         压制设置，格式为：帧大小（例如1920x1080）,帧率（例如25）,码率（例如6000）,峰值码率（例如24000）；所有值可缺省但需保留逗号，缺省值默认为B站投稿最高配置。
-f                              忽略错误，默认为遇到错误即结束
```


## 开放源代码许可
### Newtonsoft.Json 12.0.2
<https://www.newtonsoft.com/json>

Copyright (c) 2019 Newtonsoft

Licensed under MIT

### ffmpeg v4.0
<http://ffmpeg.org/>

Copyright (c) 2000-2018 the FFmpeg developers

Licensed under LGPL v2.1 or later

