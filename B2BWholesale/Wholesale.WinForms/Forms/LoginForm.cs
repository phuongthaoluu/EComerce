using System;
using System.Drawing;
using System.Windows.Forms;
using Wholesale.WinForms.Data;
using Wholesale.WinForms.Security;
using Wholesale.WinForms.Services;
using Wholesale.WinForms.UI;

namespace Wholesale.WinForms.Forms
{
    /// <summary>Task 1.1 - secure login. Only active Admin users can continue.</summary>
    public class LoginForm : Form
    {
        private readonly TextBox _txtUser = new TextBox();
        private readonly TextBox _txtPass = new TextBox();
        private readonly CheckBox _chkShow = new CheckBox();
        private readonly Label _lblMsg = new Label();
        private readonly Button _btnLogin;
        private readonly Button _btnExit;

        public LoginForm()
        {
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = "Wholesale Admin - Sign in";
            ClientSize = new Size(760, 440);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Font = Theme.BaseFont;
            BackColor = Color.White;

            var left = new Panel { Left = 0, Top = 0, Width = 300, Height = 440, BackColor = Theme.Navy };
            var brand = new Label
            {
                Text = "B2B Wholesale\nBack-Office",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                AutoSize = false,
                Left = 28, Top = 130, Width = 250, Height = 90
            };
            var sub = new Label
            {
                Text = "Warehouse administration:\ninventory, agents, orders and sales analysis.",
                ForeColor = Color.FromArgb(156, 163, 175),
                AutoSize = false,
                Left = 28, Top = 230, Width = 250, Height = 60
            };
            left.Controls.Add(brand);
            left.Controls.Add(sub);
            Controls.Add(left);

            var title = new Label { Text = "Administrator sign in", Font = Theme.TitleFont, ForeColor = Theme.Navy, AutoSize = true, Left = 340, Top = 60 };
            var hint = new Label { Text = "Only users with the Admin role can use this application.", ForeColor = Theme.TextMuted, AutoSize = true, Left = 342, Top = 98 };
            var lblUser = new Label { Text = "Username or email", Font = Theme.BoldFont, ForeColor = Theme.Navy, AutoSize = true, Left = 342, Top = 140 };
            _txtUser.SetBounds(342, 164, 380, 28);
            _txtUser.Font = new Font("Segoe UI", 11f);
            var lblPass = new Label { Text = "Password", Font = Theme.BoldFont, ForeColor = Theme.Navy, AutoSize = true, Left = 342, Top = 204 };
            _txtPass.SetBounds(342, 228, 380, 28);
            _txtPass.Font = new Font("Segoe UI", 11f);
            _txtPass.UseSystemPasswordChar = true;
            _chkShow.Text = "Show password";
            _chkShow.AutoSize = true;
            _chkShow.Left = 342;
            _chkShow.Top = 264;
            _chkShow.CheckedChanged += (s, e) => _txtPass.UseSystemPasswordChar = !_chkShow.Checked;

            _lblMsg.SetBounds(342, 294, 380, 44);
            _lblMsg.ForeColor = Theme.Danger;
            _lblMsg.AutoSize = false;

            _btnLogin = Theme.MakeButton("Sign in", Theme.Primary, 180);
            _btnLogin.SetBounds(342, 346, 180, 38);
            _btnLogin.Click += OnLogin;
            _btnExit = Theme.MakeButton("Exit", Theme.Grey, 120);
            _btnExit.SetBounds(534, 346, 120, 38);
            _btnExit.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.AddRange(new Control[] { title, hint, lblUser, _txtUser, lblPass, _txtPass, _chkShow, _lblMsg, _btnLogin, _btnExit });
            AcceptButton = _btnLogin;
            CancelButton = _btnExit;
        }

        private void OnLogin(object sender, EventArgs e)
        {
            _lblMsg.Text = "";
            _btnLogin.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                LoginResult result = LoginService.Login(_txtUser.Text, _txtPass.Text);
                if (result.Status == LoginStatus.Success)
                {
                    Session.CurrentUser = result.User;
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }
                _lblMsg.Text = result.Message;
                _txtPass.Clear();
                _txtPass.Focus();
            }
            catch (Exception ex)
            {
                _lblMsg.Text = Db.Friendly(ex);
            }
            finally
            {
                Cursor = Cursors.Default;
                _btnLogin.Enabled = true;
            }
        }
    }
}
