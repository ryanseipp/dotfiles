local has_lsp, lspconfig = pcall(require, 'lspconfig')
if not has_lsp then
    return
end

-- add commands to run when init every langserver
local custom_init = function(client)
    client.config.flags = client.config.flags or {}
    client.config.flags.allow_incremental_sync = true
end

local augroup_highlight = vim.api.nvim_create_augroup("custom-lsp-references", { clear = true })
local augroup_codelens = vim.api.nvim_create_augroup("custom-lsp-codelens", { clear = true })
local augroup_format = vim.api.nvim_create_augroup("custom-lsp-format", { clear = true })
local augroup_eslint_fixall = vim.api.nvim_create_augroup("custom-lsp-format", { clear = true })

local autocmd_format = function(async, filter)
    vim.api.nvim_clear_autocmds { buffer = 0, group = augroup_format }
    vim.api.nvim_create_autocmd("BufWritePre", {
        group = augroup_format,
        buffer = 0,
        callback = function()
            vim.lsp.buf.format { async = async, filter = filter }
        end
    })
end

local autocmd_eslint_fixall = function()
    vim.api.nvim_clear_autocmds { buffer = 0, group = augroup_eslint_fixall }
    vim.api.nvim_create_autocmd("BufWritePre", {
        group = augroup_eslint_fixall,
        buffer = 0,
        callback = function()
            local clients = vim.lsp.get_active_clients()
            for _, v in pairs(clients) do
                if v.name == 'eslint' then
                    vim.cmd [[ EslintFixAll ]]
                end
            end
        end
    })
end

-- add filetype specific commands here
local filetype_attach = setmetatable({
    lua = function()
        autocmd_format(false)
    end,
    rust = function()
        autocmd_format(false)
    end,
    javascript = function()
        autocmd_eslint_fixall()
    end,
    javascriptreact = function()
        autocmd_eslint_fixall()
    end,
    ["javascript.jsx"] = function()
        autocmd_eslint_fixall()
    end,
    typescript = function()
        autocmd_eslint_fixall()
    end,
    typescriptreact = function()
        autocmd_eslint_fixall()
    end,
    ["typescript.ts"] = function()
        autocmd_eslint_fixall()
    end,
}, {
    __index = function()
        return function() end
    end,
})

local custom_attach = function(client, bufnr)
    local filetype = vim.api.nvim_buf_get_option(bufnr, 'filetype')

    local mappings = {
        ['gd'] = vim.lsp.buf.definition,
        ['gD'] = vim.lsp.buf.declaration,
        ['gT'] = vim.lsp.buf.type_definition,
        ['gr'] = vim.lsp.buf.references,
        ['K'] = vim.lsp.buf.hover,
        ['<c-k>'] = vim.lsp.buf.signature_help,
        ['<leader>rn'] = vim.lsp.buf.rename,
        ['<leader>rf'] = vim.lsp.buf.format,
        ['[d'] = vim.diagnostic.goto_next,
        [']d'] = vim.diagnostic.goto_prev,
        ['<leader>ca'] = vim.lsp.buf.code_action,
        ['<leader>rr'] = function()
            vim.cmd [[ LspRestart ]]
        end,
    }

    for keys, mapping in pairs(mappings) do
        vim.api.nvim_buf_set_keymap(bufnr, "n", keys, "", { callback = mapping, noremap = true })
    end

    vim.bo.omnifunc = "v:lua.vim.lsp.omnifunc"

    -- Set autocommands conditional on server_capabilities
    if client.server_capabilities.documentHighlightProvider then
        vim.api.nvim_clear_autocmds { group = augroup_highlight, buffer = bufnr }
        vim.api.nvim_create_autocmd("CursorHold", {
            group = augroup_highlight,
            callback = vim.lsp.buf.document_highlight,
            buffer = bufnr
        })
        vim.api.nvim_create_autocmd("CursorMoved", {
            group = augroup_highlight,
            callback = vim.lsp.buf.clear_references,
            buffer = bufnr
        })
    end

    if client.server_capabilities.codeLensProvider then
        vim.api.nvim_clear_autocmds { group = augroup_codelens, buffer = bufnr }
        vim.api.nvim_create_autocmd("BufEnter", {
            group = augroup_codelens,
            callback = vim.lsp.codelens.refresh,
            buffer = bufnr,
            once = true
        })
        vim.api.nvim_create_autocmd({ "BufWritePost", "CursorHold" }, {
            group = augroup_codelens,
            callback = vim.lsp.codelens.refresh,
            buffer = bufnr
        })
    end

    -- Attach any filetype specific options to the client
    filetype_attach[filetype](client)
end

local updated_capabilities = vim.lsp.protocol.make_client_capabilities()
updated_capabilities.textDocument.codeLens = { dynamicRegistration = false }
updated_capabilities = require('cmp_nvim_lsp').default_capabilities(updated_capabilities)

local lua_runtime_path = vim.split(package.path, ';')
table.insert(lua_runtime_path, 'lua/?.lua')
table.insert(lua_runtime_path, 'lua/?/init.lua')

-- define lang server configs
local servers = {
    ansiblels = (1 == vim.fn.executable 'ansible-language-server'),
    astro = (1 == vim.fn.executable 'astro-ls'),
    bashls = (1 == vim.fn.executable 'bash-language-server'),
    cmake = (1 == vim.fn.executable 'cmake-language-server'),
    dockerls = (1 == vim.fn.executable 'docker-langserver'),
    gopls = (1 == vim.fn.executable 'gople'),
    pylsp = (1 == vim.fn.executable 'pylsp'),
    terraformls = (1 == vim.fn.executable 'terraform-ls'),
    tailwindcss = (1 == vim.fn.executable 'tailwindcss-language-server'),
    tsserver = (1 == vim.fn.executable 'typescript-language-server'),
    yamlls = (1 == vim.fn.executable 'yaml-language-server'),

    clangd = {
        cmd = {
            'clangd',
            '--background-index',
            '--clang-tidy',
            '--header-insertion=iwyu',
        },
        init_options = {
            clangdFileStatus = true,
        },
        root_dir = lspconfig.util.root_pattern('*/**/compile_commands.json'),
    },

    eslint = {
        settings = {
            packageManager = 'yarn',
            format = false,
        },
    },

    hls = {
        root_dir = lspconfig.util.root_pattern('*'),
        settings = {
            haskell = {
                hlintOn = true,
                formattingProvider = "stylish-haskell"
            }
        }
    },

    omnisharp = {
        cmd = { '/usr/bin/omnisharp', '-lsp', '--hostPID', tostring(vim.fn.getpid()) },
        enable_import_completion = true,
        enable_decompilation_support = true,
        organize_imports_on_format = true,
        root_dir = lspconfig.util.root_pattern('*.sln'),
    },

    rust_analyzer = {
        settings = {
            cargo = {
                buildScripts = {
                    enable = true
                }
            }
        }
    },

    sumneko_lua = {
        cmd = { '/usr/bin/lua-language-server' };
        settings = {
            Lua = {
                runtime = {
                    -- Tell the language server which version of Lua you're using (most likely LuaJIT in the case of Neovim)
                    version = 'LuaJIT',
                    -- Setup your lua path
                    path = lua_runtime_path,
                },
                diagnostics = {
                    -- Get the language server to recognize the `vim` global
                    globals = { 'vim', 'awesome', 'client', 'root' },
                },
                workspace = {
                    -- Make the server aware of Neovim runtime files
                    library = {
                        ['/usr/share/nvim/runtime/lua'] = true,
                        ['/usr/share/nvim/runtime/lua/vim/lsp'] = true,
                        ['/usr/share/awesome/lib'] = true
                    },
                },
                telemetry = {
                    enable = false,
                },
            },
        },
    },
}

local setup_server = function(server, config)
    if not config then
        return
    end

    if type(config) ~= 'table' then
        config = {}
    end

    config = vim.tbl_deep_extend('force', {
        on_init = custom_init,
        on_attach = custom_attach,
        capabilities = updated_capabilities,
        flags = {
            debounce_text_changes = 50,
        },
    }, config)

    lspconfig[server].setup(config)
end

for server, config in pairs(servers) do
    setup_server(server, config)
end

require('rs.lsp.null-ls').setup(custom_attach)

return {
    on_init = custom_init,
    on_attach = custom_attach,
    capabilities = updated_capabilities,
}
