local has_dap, dap = pcall(require, 'dap')
if not has_dap then
    return
end

local has_nvim_dap_virt_text, dap_virt_text = pcall(require, 'nvim-dap-virtual-text')
if has_nvim_dap_virt_text then
    dap_virt_text.setup {
        enabled_commands = false
    }
end

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

dap.adapters.coreclr = {
    type = 'executable',
    command = 'netcoredbg',
    args = {'--interpreter=vscode'}
}
dap.configurations.cs = {
    {
        type = 'coreclr',
        name = 'launch - netcoredbg',
        request = 'launch',
        program = function()
            return vim.fn.input('Path to dll', vim.fn.getcwd() .. '/bin/Debug/', 'file')
        end,
    }
}

dap.adapters.firefox = {
    type = 'executable',
    command = 'node',
    args = {os.getenv('HOME') .. '/.local/share/vscode-firefox-debug/dist/adapter.bundle.js'},
}
dap.configurations.typescript = {
    name = 'Debug with Firefox',
    type = 'firefox',
    request = 'launch',
    reAttach = true,
    url = 'http://localhost:8000',
    webRoot = '${workspaceFolder}',
    firefoxExecutable = '/usr/bin/firefox'
}
dap.configurations.typescriptreact = {
    name = 'Debug with Firefox',
    type = 'firefox',
    request = 'launch',
    reAttach = true,
    url = 'http://localhost:8000',
    webRoot = '${workspaceFolder}',
    firefoxExecutable = '/usr/bin/firefox'
}

local map = function(lhs, rhs, desc)
    if desc then
        desc = "[DAP] " .. desc
    end

    vim.keymap.set("n", lhs, rhs, {silent = true, desc = desc})
end

map("<F1>", dap.step_back, "step_back")
map("<F2>", dap.step_into, "step_into")
map("<F3>", dap.step_over, "step_over")
map("<F4>", dap.step_out, "step_out")
map("<F5>", dap.continue, "continue")

map("<leader>dr", dap.repl.open, "repl_open")

map("<leader>db", dap.toggle_breakpoint, "toggle_breakpoint")
map("<leader>dB", function()
    dap.set_breakpoint(vim.fn.input "[DAP] Condition > ")
end)

local has_dapui, dapui = pcall(require, 'dapui')
if has_dapui then
    dapui.setup()

    map("<leader>de", dapui.eval, "eval")
    map("<leader>dE", function()
        dapui.eval(vim.fn.input "[DAP] Expression > ")
    end)


    dap.listeners.after.event_initialized["dapui_config"] = function()
        dapui.open({})
    end

    dap.listeners.before.event_terminated["dapui_config"] = function()
        dapui.close({})
    end

    dap.listeners.before.event_exited["dapui_config"] = function()
        dapui.close({})
    end
end

