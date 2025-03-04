using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MediaControlDistributionCenter.Helpers
{
    public enum TipType
    {
        [Description("接口及方法调用")]
        Method,

        [Description("资源错误")]
        Resourc,

        [Description("输入验证")]
        Input,

        [Description("用户验证")]
        User,

        [Description("权限不足")]
        Permissions

        //有需要自己扩展
    }
    public class TipCode
    {
        public int Code { get; set; }

        public TipType Type { get; set; }

        public string Description { get; set; }
        /// <summary>
        /// 由于国际化所以可以根据默认的编码查找到对应的中文提示
        /// </summary>
        public string LanguageKey { get; set; }
    }
    public static class TipCodeFindRes
    {
        public static List<TipCode> tipTypes = new List<TipCode>() {
        new  TipCode(){ Code=100,Description="参数无效或缺失", LanguageKey="100", Type=TipType.Method},
new  TipCode(){ Code=102,Description="没有相关数据", LanguageKey="102", Type=TipType.Method},
new  TipCode(){ Code=104,Description="调用端的IP未被授权", LanguageKey="104", Type=TipType.Method},
new  TipCode(){ Code=106,Description="接口不被支持", LanguageKey="106", Type=TipType.Method},
new  TipCode(){ Code=108,Description="服务暂时不可用", LanguageKey="108", Type=TipType.Method},
new  TipCode(){ Code=110,Description="参数过多", LanguageKey="110", Type=TipType.Method},
new  TipCode(){ Code=112,Description="没有权限访问数据", LanguageKey="112", Type=TipType.Method},
new  TipCode(){ Code=114,Description="修改失败", LanguageKey="114", Type=TipType.Method},
new  TipCode(){ Code=116,Description="删除失败", LanguageKey="116", Type=TipType.Method},
new  TipCode(){ Code=118,Description="添加失败", LanguageKey="118", Type=TipType.Method},
new  TipCode(){ Code=120,Description="操作失败", LanguageKey="120", Type=TipType.Method},
new  TipCode(){ Code=122,Description="接口请求太过频繁", LanguageKey="122", Type=TipType.Method},
new  TipCode(){ Code=124,Description="应用不存在", LanguageKey="124", Type=TipType.Method},
new  TipCode(){ Code=126,Description="算法未被平台所支持", LanguageKey="126", Type=TipType.Method},
new  TipCode(){ Code=128,Description="发送信息失败", LanguageKey="128", Type=TipType.Method},
new  TipCode(){ Code=130,Description="用户级自定义错误", LanguageKey="130", Type=TipType.Method},
new  TipCode(){ Code=132,Description="必选参数格式错误", LanguageKey="132", Type=TipType.Method},
new  TipCode(){ Code=134,Description="文件错误", LanguageKey="134", Type=TipType.Method},
new  TipCode(){ Code=136,Description="HTTP方法被禁止", LanguageKey="136", Type=TipType.Method},
new  TipCode(){ Code=138,Description="服务不可用", LanguageKey="138", Type=TipType.Method},
new  TipCode(){ Code=140,Description="未检索到匹配的资源", LanguageKey="140", Type=TipType.Method},
new  TipCode(){ Code=142,Description="资源已经创建", LanguageKey="142", Type=TipType.Method},
new  TipCode(){ Code=144,Description="调用异常", LanguageKey="144", Type=TipType.Method},
new  TipCode(){ Code=146,Description="内部异常", LanguageKey="146", Type=TipType.Method},
new  TipCode(){ Code=148,Description="正在升级", LanguageKey="148", Type=TipType.Method},
new  TipCode(){ Code=150,Description="需要GET请求", LanguageKey="150", Type=TipType.Method},
new  TipCode(){ Code=152,Description="需要POST请求", LanguageKey="152", Type=TipType.Method},
new  TipCode(){ Code=154,Description="需要TCP请求", LanguageKey="154", Type=TipType.Method},
new  TipCode(){ Code=156,Description="需要UDP请求", LanguageKey="156", Type=TipType.Method},
new  TipCode(){ Code=200,Description="端口未开放", LanguageKey="200", Type=TipType.Resourc},
new  TipCode(){ Code=202,Description="线程死锁", LanguageKey="202", Type=TipType.Resourc},
new  TipCode(){ Code=204,Description="隐性死锁", LanguageKey="204", Type=TipType.Resourc},
new  TipCode(){ Code=206,Description="占有并等待", LanguageKey="206", Type=TipType.Resourc},
new  TipCode(){ Code=208,Description="线程并发错误", LanguageKey="208", Type=TipType.Resourc},
new  TipCode(){ Code=210,Description="无法转换数据", LanguageKey="210", Type=TipType.Resourc},
new  TipCode(){ Code=212,Description="操作正在执行", LanguageKey="212", Type=TipType.Resourc},
new  TipCode(){ Code=214,Description="文件Content-type不合法", LanguageKey="214", Type=TipType.Resourc},
new  TipCode(){ Code=216,Description="无法解析文件", LanguageKey="216", Type=TipType.Resourc},
new  TipCode(){ Code=218,Description="循环陷入死循环", LanguageKey="218", Type=TipType.Resourc},
new  TipCode(){ Code=220,Description="空指针错误", LanguageKey="220", Type=TipType.Resourc},
new  TipCode(){ Code=222,Description="无法访问", LanguageKey="222", Type=TipType.Resourc},
new  TipCode(){ Code=224,Description="数组越界", LanguageKey="224", Type=TipType.Resourc},
new  TipCode(){ Code=226,Description="除数不能为0", LanguageKey="226", Type=TipType.Resourc},
new  TipCode(){ Code=228,Description="无法释放资源", LanguageKey="228", Type=TipType.Resourc},
new  TipCode(){ Code=300,Description="输入无效", LanguageKey="300", Type=TipType.Input},
new  TipCode(){ Code=302,Description="正则表达式验证失败", LanguageKey="302", Type=TipType.Input},
new  TipCode(){ Code=304,Description="必须选择一项", LanguageKey="304", Type=TipType.Input},
new  TipCode(){ Code=306,Description="输入值不符合email", LanguageKey="306", Type=TipType.Input},
new  TipCode(){ Code=308,Description="输入值不符合url", LanguageKey="308", Type=TipType.Input},
new  TipCode(){ Code=310,Description="输入不能为空", LanguageKey="310", Type=TipType.Input},
new  TipCode(){ Code=312,Description="非负整数", LanguageKey="312", Type=TipType.Input},
new  TipCode(){ Code=314,Description="非正整数", LanguageKey="314", Type=TipType.Input},
new  TipCode(){ Code=316,Description="密码必须是8-16个字符之间且包含大小写", LanguageKey="316", Type=TipType.Input},
new  TipCode(){ Code=318,Description="非正浮点数", LanguageKey="318", Type=TipType.Input},
new  TipCode(){ Code=320,Description="非负浮点数", LanguageKey="320", Type=TipType.Input},
new  TipCode(){ Code=322,Description="只能是汉字", LanguageKey="322", Type=TipType.Input},
new  TipCode(){ Code=324,Description="只能是英文", LanguageKey="324", Type=TipType.Input},
new  TipCode(){ Code=326,Description="两次输入密码不相同", LanguageKey="326", Type=TipType.Input},
new  TipCode(){ Code=328,Description="电话格式不正确", LanguageKey="328", Type=TipType.Input},
new  TipCode(){ Code=330,Description="手机号码格式不正确", LanguageKey="330", Type=TipType.Input},
new  TipCode(){ Code=332,Description="身份证号码格式不正确", LanguageKey="332", Type=TipType.Input},
new  TipCode(){ Code=334,Description="银行账户格式不正确", LanguageKey="334", Type=TipType.Input},
new  TipCode(){ Code=336,Description="邮政编码格式不正确", LanguageKey="336", Type=TipType.Input},
new  TipCode(){ Code=338,Description="俯仰角度必须是正负90度", LanguageKey="338", Type=TipType.Input},
new  TipCode(){ Code=340,Description="偏航角度必须是正负90度", LanguageKey="340", Type=TipType.Input},
new  TipCode(){ Code=342,Description="地理信息必须是正负180度", LanguageKey="342", Type=TipType.Input},
new  TipCode(){ Code=344,Description="位置信息必须是正负180度", LanguageKey="344", Type=TipType.Input},
new  TipCode(){ Code=346,Description="照片应该为JPG,BMP,PNG格式的", LanguageKey="346", Type=TipType.Input},
new  TipCode(){ Code=348,Description="文件格式不正确", LanguageKey="348", Type=TipType.Input},
new  TipCode(){ Code=350,Description="日期格式不正确", LanguageKey="350", Type=TipType.Input},
new  TipCode(){ Code=352,Description="IP地址格式不正确", LanguageKey="352", Type=TipType.Input},
new  TipCode(){ Code=354,Description="端口号格式不正确", LanguageKey="354", Type=TipType.Input},
new  TipCode(){ Code=356,Description="输入日期小于最小日期", LanguageKey="356", Type=TipType.Input},
new  TipCode(){ Code=358,Description="输入日期大于最大日期", LanguageKey="358", Type=TipType.Input},
new  TipCode(){ Code=360,Description="输入值超过最大的限定", LanguageKey="360", Type=TipType.Input},
new  TipCode(){ Code=362,Description="输入值小于最小的限定", LanguageKey="362", Type=TipType.Input},
new  TipCode(){ Code=364,Description="输入的字符数超过最大长度", LanguageKey="364", Type=TipType.Input},
new  TipCode(){ Code=366,Description="输入的字符数小于最小长度", LanguageKey="366", Type=TipType.Input},
new  TipCode(){ Code=368,Description="输入的数字不符合step限制", LanguageKey="368", Type=TipType.Input},
new  TipCode(){ Code=370,Description="验证码错误", LanguageKey="370", Type=TipType.Input},
new  TipCode(){ Code=372,Description="验证码已过期", LanguageKey="372", Type=TipType.Input},
new  TipCode(){ Code=400,Description="用户未登录", LanguageKey="400", Type=TipType.User},
new  TipCode(){ Code=402,Description="用户名或密码错误", LanguageKey="402", Type=TipType.User},
new  TipCode(){ Code=404,Description="用户名不存在", LanguageKey="404", Type=TipType.User},
new  TipCode(){ Code=406,Description="用户名必须是英文", LanguageKey="406", Type=TipType.User},
new  TipCode(){ Code=408,Description="用户密码不能为空", LanguageKey="408", Type=TipType.User},
new  TipCode(){ Code=410,Description="该用户名已经存在", LanguageKey="410", Type=TipType.User},
new  TipCode(){ Code=412,Description="用户注册失败", LanguageKey="412", Type=TipType.User},
new  TipCode(){ Code=414,Description="用户未设置密码", LanguageKey="414", Type=TipType.User},
new  TipCode(){ Code=416,Description="该账号已停用", LanguageKey="416", Type=TipType.User},
new  TipCode(){ Code=418,Description="用户密码错误", LanguageKey="418", Type=TipType.User},
new  TipCode(){ Code=501,Description="用户权限不足", LanguageKey="501", Type=TipType.Permissions},
new  TipCode(){ Code=502,Description="用户会话已过期", LanguageKey="502", Type=TipType.Permissions},
new  TipCode(){ Code=503,Description="用户操作过于频繁", LanguageKey="503", Type=TipType.Permissions},
new  TipCode(){ Code=504,Description="用户被锁定", LanguageKey="504", Type=TipType.Permissions},
new  TipCode(){ Code=505,Description="用户尝试次数过多", LanguageKey="505", Type=TipType.Permissions},
new  TipCode(){ Code=506,Description="用户账户异常", LanguageKey="506", Type=TipType.Permissions},
new  TipCode(){ Code=507,Description="用户认证失败", LanguageKey="507", Type=TipType.Permissions},
new  TipCode(){ Code=508,Description="用户角色不存在", LanguageKey="508", Type=TipType.Permissions},
new  TipCode(){ Code=509,Description="用户权限被撤销", LanguageKey="509", Type=TipType.Permissions},
new  TipCode(){ Code=510,Description="用户账户已注销", LanguageKey="510", Type=TipType.Permissions},
new  TipCode(){ Code=511,Description="用户账户已冻结", LanguageKey="511", Type=TipType.Permissions},
new  TipCode(){ Code=512,Description="用户账户已禁用", LanguageKey="512", Type=TipType.Permissions},
new  TipCode(){ Code=513,Description="用户账户已过期", LanguageKey="513", Type=TipType.Permissions},
new  TipCode(){ Code=514,Description="用户账户未激活", LanguageKey="514", Type=TipType.Permissions},
new  TipCode(){ Code=515,Description="用户账户未验证", LanguageKey="515", Type=TipType.Permissions},
new  TipCode(){ Code=516,Description="用户账户未绑定邮箱", LanguageKey="516", Type=TipType.Permissions},
new  TipCode(){ Code=517,Description="用户账户未绑定手机号", LanguageKey="517", Type=TipType.Permissions},
new  TipCode(){ Code=518,Description="用户账户未完成安全验证", LanguageKey="518", Type=TipType.Permissions},
new  TipCode(){ Code=519,Description="用户账户未完成实名认证", LanguageKey="519", Type=TipType.Permissions},
new  TipCode(){ Code=520,Description="用户账户未完成邮箱验证", LanguageKey="520", Type=TipType.Permissions},
new  TipCode(){ Code=521,Description="用户账户未完成手机号验证", LanguageKey="521", Type=TipType.Permissions},
new  TipCode(){ Code=522,Description="用户账户未完成身份验证", LanguageKey="522", Type=TipType.Permissions},
new  TipCode(){ Code=523,Description="用户账户未完成安全设置", LanguageKey="523", Type=TipType.Permissions},
new  TipCode(){ Code=524,Description="用户账户未完成安全绑定", LanguageKey="524", Type=TipType.Permissions},
new  TipCode(){ Code=525,Description="用户账户未完成安全认证", LanguageKey="525", Type=TipType.Permissions},
new  TipCode(){ Code=526,Description="用户账户未完成安全审核", LanguageKey="526", Type=TipType.Permissions},

        };

        /// <summary>
        /// 依据编码返回对应的错误描述
        /// </summary>
        /// <param name="code">错误编码</param>
        /// <returns></returns>
        public static string GetTipString(int code)
        {
            var temp = tipTypes.Where(d => d.Code == code).FirstOrDefault();
            if (temp != null)
            {
                string key = "LanguageKey_Code_Totip_" + temp.LanguageKey;
                return Application.Current.TryFindResource(key)?.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

    }
}
