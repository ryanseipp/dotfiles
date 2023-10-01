return {
    "stevearc/conform.nvim",
    event = { "BufReadPre", "BufNewFile" },
    opts = {
        formatters_by_ft = {
            javascript = { "prettier" },
        }
    },
    keys = {
        {
            "<leader>rf",
            function()
                require('conform').format({
                    lsp_fallback = true,
                    async = false,
                })
            end,
            mode = { "n", "v" },
            desc = "Format file or range (in visual mode)"
        }
    },
    config = function()
        local conform = require('conform')
        local prettier_fts = vim.tbl_map(function() return { "prettier" } end,
            { "javascript", "typescript", "javascriptreact", "typescriptreact", "svelte", "css", "html", "json", "yaml",
                "markdown", "astro" });

        conform.setup({
            formatters_by_ft = vim.tbl_extend("force", prettier_fts, {
                c = { "clang_format" },
                lua = { "stylua" },
                ocaml = { "ocamlformat" },
            }),
            format_on_save = {
                lsp_fallback = true,
                async = false,
            }
        })
    end
}
