﻿using System;
using System.Collections.Generic;

namespace qiniu
{
	public class QiniuErrors
	{
		/// <summary>
		/// See http://developer.qiniu.com/docs/v6/api/reference/codes.html
		/// </summary>
		public static Dictionary<int,string> ErrorCodes = new Dictionary<int, string> {
			{298,"部分操作执行成功"},
			{400,"请求报文格式错误"},
			{401,"认证授权失败，可能是密钥信息不正确、数字签名错误或授权已超时"},
			{404,"资源不存在"},
			{405,"请求方式错误，非预期的请求方式"},
			{406,"上传的数据 CRC32 校验错"},
			{419,"用户账号被冻结"},
			{503,"服务端不可用"},
			{504,"服务端操作超时"},
			{579,"资源上传成功，但是回调失败"},
			{599,"服务端操作失败"},
			{608,"资源内容被修改"},
			{612,"指定资源不存在或已被删除"},
			{614,"目标资源已存在"},
			{630,"已创建的空间数量达到上限，无法创建新空间"},
			{631,"指定空间不存在"},
			{701,"上传数据块校验出错"}
		};

		private int httpCode;

		/// <summary>
		/// Gets the http code.
		/// </summary>
		/// <value>The http code.</value>
		public int HttpCode {
			get { return httpCode; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="qiniu.QiniuErrors"/> class.
		/// </summary>
		/// <param name="code">Code.</param>
		public QiniuErrors (int code)
		{
			this.httpCode = code;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="qiniu.QiniuErrors"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="qiniu.QiniuErrors"/>.</returns>
		public override string ToString ()
		{
			return ErrorCodes [this.httpCode];
		}
	}
}

