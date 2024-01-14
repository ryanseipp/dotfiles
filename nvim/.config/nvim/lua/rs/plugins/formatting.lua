return {
    "stevearc/conform.nvim",
    event = { "BufReadPre", "BufNewFile" },
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
        local prettier_fts = { "javascript", "typescript", "javascriptreact", "typescriptreact", "svelte", "css", "html",
            "json", "yaml", "markdown" }

        local formatters_by_ft = {
            c = { "clang_format" },
            cs = { "csharpier" },
            lua = { "stylua" },
            ocaml = { "ocamlformat" },
        }

        for _, value in ipairs(prettier_fts) do
            formatters_by_ft[value] = { "prettier" }
        end

        conform.setup({
            formatters_by_ft = formatters_by_ft,
            format_on_save = {
                lsp_fallback = true,
                async = false,
            }
        })
    end
}
