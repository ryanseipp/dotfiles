return {
    {
        'mfussenegger/nvim-dap',
        keys = {
            { '<F1>', "<cmd>lua require('dap').step_back()<cr>", desc = "[DAP] step_back", silent = true },
            { '<F2>', "<cmd>lua require('dap').step_into()<cr>", desc = "[DAP] step_into", silent = true },
            { '<F3>', "<cmd>lua require('dap').step_over()<cr>", desc = "[DAP] step_over", silent = true },
            { '<F4>', "<cmd>lua require('dap').step_out()<cr>",  desc = "[DAP] step_out",  silent = true },
            { '<F5>', "<cmd>lua require('dap').continue()<cr>",  desc = "[DAP] continue",  silent = true },
            {
                '<leader><F5>',
                function()
                    if vim.bo.filetype ~= "rust" then
                        vim.notify "This wasn't rust. I don't know what to do"
                        return
                    end

                    require("rs.dap").select_rust_runnable()
                end,
                desc = "[DAP] Run language runnable",
                silent = true,
            },
            {
                '<leader>dr',
                "<cmd>lua require('dap').repl.open()<cr>",
                desc = "[DAP] repl_open",
                silent = true
            },
            {
                '<leader>db',
                "<cmd>lua require('dap').toggle_breakpoint()<cr>",
                desc = "[DAP] toggle_breakpoint",
                silent = true
            },
            {
                '<leader>dr',
                "<cmd>lua require('dap').set_breakpoint(vim.fn.input '[DAP] Condition > ')<cr>",
                desc = "[DAP] set breakpoint condition",
                silent = true
            },
        },
        dependencies = {
            {
                'nvim-telescope/telescope-dap.nvim',
                'theHamsta/nvim-dap-virtual-text',
                config = {
                    enabled = true,
                    enabled_commands = false,
                    highlight_changed_variables = true,
                    highlight_new_as_changes = true,
                    commented = false,
                    show_stop_reason = true,
                }
            },
        },
        config = function()
            vim.fn.sign_define('DapBreakpoint', { text = '●', texthl = '', linehl = '', numhl = '' })
            vim.fn.sign_define('DapBreakpointCondition', { text = '◎', texthl = '', linehl = '', numhl = '' })
            vim.fn.sign_define('DapStopped', { text = '➡', texthl = '', linehl = '', numhl = '' })
            vim.fn.sign_define('DapBreakpointRejected', { text = '○', texthl = '', linehl = '', numhl = '' })

            local dap = require('dap')
            dap.defaults.fallback.external_terminal = {
                command = '/usr/bin/alacritty',
                args = { '-e' },
            }

            dap.adapters.lldb = {
                type = 'executable',
                command = "/usr/bin/lldb-vscode",
                name = "lldb"
            }
            dap.configurations.rust = { {
                name = "Launch lldb",
                type = "lldb",
                request = "launch",
                program = function()
                    return vim.fn.input("Path to executable: ", vim.fn.getcwd() .. "/", "file")
                end,
                cwd = "${workspaceFolder}",
                stopOnEntry = false,
                args = {},
                runInTerminal = false,
            } }
            dap.configurations.c = dap.configurations.rust

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
                args = { '--interpreter=vscode' }
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
                args = { os.getenv('HOME') .. '/.local/share/vscode-firefox-debug/dist/adapter.bundle.js' },
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

            dap.listeners.after.event_initialized["dapui_config"] = function()
                require('dapui').open({})
            end

            dap.listeners.before.event_terminated["dapui_config"] = function()
                require('dapui').close({})
            end

            dap.listeners.before.event_exited["dapui_config"] = function()
                require('dapui').close({})
            end
        end
    },
    {
        'rcarriga/nvim-dap-ui',
        keys = {
            { '<leader>de', "<cmd>lua require('dapui').eval()<cr>", desc = '[DAP] eval' },
            {
                '<leader>dE',
                "<cmd>lua require('dapui').eval(vim.fn.input('[DAP] Expression > '))<cr>",
                desc = '[DAP] eval expression'
            }
        },
        config = true
    },
}
