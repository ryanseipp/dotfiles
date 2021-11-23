if require "rs.first_load"() then
  return
end

vim.g.mapleader = "<Space>"

require "rs.plugins"
require "rs.colors"
require "rs.lsp"
require "rs.completions"

