return {
    { 'williamboman/mason.nvim', opts = {}, cmd = 'Mason' },
    {
        'neovim/nvim-lspconfig',
        event = { "BufReadPre", "BufNewFile" },
        dependencies = {
            { 'j-hui/fidget.nvim', opts = {} },
            { 'folke/neodev.nvim', opts = {} },
            {
                'williamboman/mason-lspconfig.nvim',
                dependencies = { 'williamboman/mason.nvim', opts = {} },
                opts = { automatic_installation = true, ensure_installed = { 'rust_analyzer' } },
            },
        },
        config = function()
            local lspconfig = require('lspconfig')
            local lsp = require('rs.lsp')

            local setup_server = function(server, config)
                if not config then
                    return
                end

                if type(config) ~= 'table' then
                    config = {}
                end

                config = vim.tbl_deep_extend('force', {
                    on_init = lsp.custom_init,
                    on_attach = lsp.custom_attach,
                    capabilities = lsp.capabilities,
                    flags = {
                        debounce_text_changes = 50,
                    },
                }, config)

                lspconfig[server].setup(config)
            end

            for server, config in pairs(lsp.servers) do
                setup_server(server, config)
            end
        end
    },
    {
        'mrcjkb/rustaceanvim',
        version = '^3',
        ft = { 'rust' },
        init = function()
            local lsp = require('rs.lsp')
            vim.g.rustaceanvim = {
                server = {
                    on_attach = lsp.custom_attach,
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
                }
            }
        end
    },
    {
        'pmizio/typescript-tools.nvim',
        ft = { 'typescript', 'typescriptreact' },
        dependencies = { 'nvim-lua/plenary.nvim', 'neovim/nvim-lspconfig' },
        opts = function()
            local lsp = require('rs.lsp')
            return {
                on_init = lsp.custom_init,
                on_attach = lsp.custom_attach
            }
        end
    },
}
