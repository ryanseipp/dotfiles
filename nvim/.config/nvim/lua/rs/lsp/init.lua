local has_lsp, lspconfig = pcall(require, 'lspconfig')
if not has_lsp then
    return
end

local M = {}

-- add commands to run when init every langserver
M.custom_init = function(client)
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
        return function()
        end
    end,
})

M.custom_attach = function(client, bufnr)
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

    if client.name == "omnisharp" then
        client.server_capabilities.semanticTokensProvider = {
            full = vim.empty_dict(),
            legend = {
                tokenModifiers = { "static_symbol" },
                tokenTypes = {
                    "comment",
                    "excluded_code",
                    "identifier",
                    "keyword",
                    "keyword_control",
                    "number",
                    "operator",
                    "operator_overloaded",
                    "preprocessor_keyword",
                    "string",
                    "whitespace",
                    "text",
                    "static_symbol",
                    "preprocessor_text",
                    "punctuation",
                    "string_verbatim",
                    "string_escape_character",
                    "class_name",
                    "delegate_name",
                    "enum_name",
                    "interface_name",
                    "module_name",
                    "struct_name",
                    "type_parameter_name",
                    "field_name",
                    "enum_member_name",
                    "constant_name",
                    "local_name",
                    "parameter_name",
                    "method_name",
                    "extension_method_name",
                    "property_name",
                    "event_name",
                    "namespace_name",
                    "label_name",
                    "xml_doc_comment_attribute_name",
                    "xml_doc_comment_attribute_quotes",
                    "xml_doc_comment_attribute_value",
                    "xml_doc_comment_cdata_section",
                    "xml_doc_comment_comment",
                    "xml_doc_comment_delimiter",
                    "xml_doc_comment_entity_reference",
                    "xml_doc_comment_name",
                    "xml_doc_comment_processing_instruction",
                    "xml_doc_comment_text",
                    "xml_literal_attribute_name",
                    "xml_literal_attribute_quotes",
                    "xml_literal_attribute_value",
                    "xml_literal_cdata_section",
                    "xml_literal_comment",
                    "xml_literal_delimiter",
                    "xml_literal_embedded_expression",
                    "xml_literal_entity_reference",
                    "xml_literal_name",
                    "xml_literal_processing_instruction",
                    "xml_literal_text",
                    "regex_comment",
                    "regex_character_class",
                    "regex_anchor",
                    "regex_quantifier",
                    "regex_grouping",
                    "regex_alternation",
                    "regex_text",
                    "regex_self_escaped_character",
                    "regex_other_escape",
                },
            },
            range = true,
        }
    end

    -- Attach any filetype specific options to the client
    filetype_attach[filetype](client)
end

local updated_capabilities = vim.lsp.protocol.make_client_capabilities()
updated_capabilities.textDocument.codeLens = { dynamicRegistration = false }
updated_capabilities = require('cmp_nvim_lsp').default_capabilities(updated_capabilities)

M.capabilities = updated_capabilities

local lua_runtime_path = vim.split(package.path, ';')
table.insert(lua_runtime_path, 'lua/?.lua')
table.insert(lua_runtime_path, 'lua/?/init.lua')

-- define lang server configs
M.servers = {
    ansiblels = true,
    astro = true,
    bashls = true,
    -- bufls = true,
    cmake = true,
    dockerls = true,
    -- gopls = true,
    pylsp = true,
    terraformls = true,
    tailwindcss = true,
    tsserver = true,
    yamlls = true,
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
        filetypes = { "c", "cpp", "objc", "objcpp", "cuda" },
        root_dir = lspconfig.util.root_pattern('*/**/compile_commands.json'),
    },
    eslint = {
        settings = {
            packageManager = 'pnpm',
            format = false,
        },
    },
    -- omnisharp = {
    --     cmd = { '/Users/ryanseipp/.local/share/nvim/mason/bin/omnisharp' },
    --     enable_editorconfig_support = true,
    --     enable_import_completion = true,
    --     enable_decompilation_support = true,
    --     enable_default_content_items = false,
    --     organize_imports_on_format = true,
    --     root_dir = lspconfig.util.root_pattern('*.sln'),
    -- },
    csharp_ls = {
        root_dir = lspconfig.util.root_pattern('*.sln'),
    },

    rust_analyzer = {
        settings = {
            ["rust-analyzer"] = {
                cargo = {
                    buildScripts = {
                        enable = true
                    }
                },
                checkOnSave = {
                    command = "clippy"
                }
            }
        }
    },
    lua_ls = {
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

return M
