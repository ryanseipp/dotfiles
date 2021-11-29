if require "rs.first_load"() then
  return
end

vim.g.mapleader = " "

vim.cmd [[ runtime plugin/astronauta.vim ]]

require "rs.plugins"
require "rs.lsp"

