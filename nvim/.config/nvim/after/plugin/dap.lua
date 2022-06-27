local has_dap, dap = pcall(require, 'dap')
if not has_dap then
    return
end

require('nvim-dap-virtual-text').setup {
    enabled_commands = false
}

require('dapui').setup()

vim.fn.sign_define('DapBreakpoint', {text='●', texthl='', linehl='', numhl=''})
vim.fn.sign_define('DapBreakpointCondition', {text='◎', texthl='', linehl='', numhl=''})
vim.fn.sign_define('DapStopped', {text='➡', texthl='', linehl='', numhl=''})
vim.fn.sign_define('DapBreakpointRejected', {text='○', texthl='', linehl='', numhl=''})

dap.defaults.fallback.external_terminal = {
    command = '/usr/bin/alacritty';
    args = {'-e'};
}

dap.adapters.lldb = {
    name = 'lldb',
    type = 'executable',
    command = '/usr/bin/lldb-vscode'
}
--
-- dap.configurations.cpp = {
--     {
--         name = 'Launch binary',
--         type = 'lldb',
--         request = 'launch',
--         program = function()
--             vim.fn.finddir()
--         end
--     }
-- }
